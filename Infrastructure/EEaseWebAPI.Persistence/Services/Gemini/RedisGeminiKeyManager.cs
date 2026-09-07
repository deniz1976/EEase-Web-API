using System.Security.Cryptography;
using System.Text;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace EEaseWebAPI.Persistence.Services.Gemini
{
    public sealed class RedisGeminiKeyManager : IGeminiKeyManager
    {
        private const string AcquireScript = """
            local capacity = tonumber(ARGV[1])
            local refillPerMs = tonumber(ARGV[2])
            local ttl = tonumber(ARGV[3])
            local start = tonumber(ARGV[4])

            local time = redis.call('TIME')
            local now = (tonumber(time[1]) * 1000) + math.floor(tonumber(time[2]) / 1000)

            local count = #KEYS
            local wait = -1

            for offset = 0, count - 1 do
                local index = ((start + offset) % count) + 1
                local bucket = KEYS[index]

                local stored = redis.call('HMGET', bucket, 'tokens', 'ts', 'cd')
                local tokens = tonumber(stored[1])
                local ts = tonumber(stored[2])
                local cooldownUntil = tonumber(stored[3]) or 0

                if tokens == nil or ts == nil then
                    tokens = capacity
                    ts = now
                end

                if now < cooldownUntil then
                    local remaining = cooldownUntil - now
                    if wait < 0 or remaining < wait then wait = remaining end
                else
                    local elapsed = now - ts
                    if elapsed > 0 then
                        tokens = math.min(capacity, tokens + (elapsed * refillPerMs))
                        ts = now
                    end

                    if tokens >= 1 then
                        redis.call('HSET', bucket, 'tokens', tokens - 1, 'ts', ts, 'cd', cooldownUntil)
                        redis.call('PEXPIRE', bucket, ttl)
                        return {index, 0}
                    end

                    redis.call('HSET', bucket, 'tokens', tokens, 'ts', ts, 'cd', cooldownUntil)
                    redis.call('PEXPIRE', bucket, ttl)

                    local remaining = (1 - tokens) / refillPerMs
                    if wait < 0 or remaining < wait then wait = remaining end
                end
            end

            if wait < 0 then wait = 50 end

            return {0, math.ceil(wait)}
            """;

        private const string ReportQuotaScript = """
            local time = redis.call('TIME')
            local now = (tonumber(time[1]) * 1000) + math.floor(tonumber(time[2]) / 1000)

            redis.call('HSET', KEYS[1], 'tokens', 0, 'ts', now, 'cd', now + tonumber(ARGV[1]))
            redis.call('PEXPIRE', KEYS[1], tonumber(ARGV[2]))

            return 1
            """;

        private readonly IConnectionMultiplexer _connection;
        private readonly IGeminiKeyManager _fallback;
        private readonly ILogger<RedisGeminiKeyManager> _logger;

        private readonly string[] _apiKeys;
        private readonly RedisKey[] _bucketKeys;
        private readonly Dictionary<string, RedisKey> _bucketKeyByApiKey;

        private readonly double _capacity;
        private readonly double _refillPerMillisecond;
        private readonly int _quotaCooldownMilliseconds;
        private readonly int _bucketTtlMilliseconds;

        private int _rotation;

        public RedisGeminiKeyManager(
            IConnectionMultiplexer connection,
            GeminiKeyManager fallback,
            IOptions<GeminiOptions> geminiOptions,
            IOptions<RedisOptions> redisOptions,
            ILogger<RedisGeminiKeyManager> logger)
        {
            _connection = connection;
            _fallback = fallback;
            _logger = logger;

            var gemini = geminiOptions.Value;
            var prefix = redisOptions.Value.InstanceName;

            _apiKeys = gemini.ApiKeys
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .ToArray();

            _bucketKeys = _apiKeys
                .Select(key => (RedisKey)$"{prefix}:gemini:key:{Fingerprint(key)}")
                .ToArray();

            _bucketKeyByApiKey = _apiKeys
                .Select((key, index) => (key, index))
                .ToDictionary(pair => pair.key, pair => _bucketKeys[pair.index]);

            _capacity = Math.Max(1, gemini.RequestsPerMinutePerKey);
            _refillPerMillisecond = _capacity / 60_000d;
            _quotaCooldownMilliseconds = gemini.QuotaCooldownSeconds * 1000;

            _bucketTtlMilliseconds = Math.Max(60_000, _quotaCooldownMilliseconds) * 2;
        }

        public async Task<string> AcquireKeyAsync(CancellationToken cancellationToken = default)
        {
            if (_apiKeys.Length == 0)
            {
                throw new GeminiAPIKeyNotFoundException();
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                RedisResult result;

                try
                {
                    var rotation = (int)((uint)Interlocked.Increment(ref _rotation) % (uint)_apiKeys.Length);

                    result = await _connection.GetDatabase().ScriptEvaluateAsync(
                        AcquireScript,
                        _bucketKeys,
                        new RedisValue[]
                        {
                            _capacity,
                            _refillPerMillisecond,
                            _bucketTtlMilliseconds,
                            rotation
                        });
                }
                catch (Exception exception) when (exception is RedisException or TimeoutException)
                {
                    _logger.LogWarning(
                        exception,
                        "Redis is unavailable, falling back to the in-process Gemini rate limiter. " +
                        "Quotas are not shared between instances while this lasts.");

                    return await _fallback.AcquireKeyAsync(cancellationToken);
                }

                var values = (RedisValue[])result!;
                var index = (int)values[0];

                if (index > 0)
                {
                    return _apiKeys[index - 1];
                }

                var waitMilliseconds = Math.Clamp((int)values[1], 10, 1000);
                await Task.Delay(waitMilliseconds, cancellationToken);
            }
        }

        public void ReportQuotaExceeded(string apiKey)
        {
            if (!_bucketKeyByApiKey.TryGetValue(apiKey, out var bucketKey))
            {
                return;
            }

            _fallback.ReportQuotaExceeded(apiKey);

            try
            {
                _connection.GetDatabase().ScriptEvaluate(
                    ReportQuotaScript,
                    new[] { bucketKey },
                    new RedisValue[] { _quotaCooldownMilliseconds, _bucketTtlMilliseconds });
            }
            catch (Exception exception) when (exception is RedisException or TimeoutException)
            {
                _logger.LogWarning(
                    exception,
                    "Could not record a Gemini quota breach in Redis; the key is only benched locally.");
            }
        }

        private static string Fingerprint(string apiKey)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes(apiKey));
            return Convert.ToHexString(hash, 0, 8).ToLowerInvariant();
        }
    }
}
