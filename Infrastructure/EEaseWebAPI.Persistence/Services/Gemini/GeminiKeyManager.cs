using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Options;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence.Services.Gemini
{
    public sealed class GeminiKeyManager : IGeminiKeyManager
    {
        private readonly object _gate = new();
        private readonly KeyState[] _keys;
        private readonly double _capacity;
        private readonly double _refillPerSecond;
        private readonly TimeSpan _quotaCooldown;

        private int _nextIndex;

        public GeminiKeyManager(IOptions<GeminiOptions> options)
        {
            var geminiOptions = options.Value;
            var now = DateTime.UtcNow;

            _capacity = Math.Max(1, geminiOptions.RequestsPerMinutePerKey);
            _refillPerSecond = _capacity / 60d;
            _quotaCooldown = TimeSpan.FromSeconds(geminiOptions.QuotaCooldownSeconds);

            _keys = geminiOptions.ApiKeys
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .Select(key => new KeyState(key, _capacity, now))
                .ToArray();
        }

        public async Task<string> AcquireKeyAsync(CancellationToken cancellationToken = default)
        {
            if (_keys.Length == 0)
            {
                throw new GeminiAPIKeyNotFoundException();
            }

            while (true)
            {
                cancellationToken.ThrowIfCancellationRequested();

                TimeSpan wait;

                lock (_gate)
                {
                    if (TryReserve(DateTime.UtcNow, out var apiKey, out wait))
                    {
                        return apiKey;
                    }
                }

                await Task.Delay(wait, cancellationToken);
            }
        }

        public void ReportQuotaExceeded(string apiKey)
        {
            lock (_gate)
            {
                var state = _keys.FirstOrDefault(key => key.ApiKey == apiKey);
                if (state is null)
                {
                    return;
                }

                var now = DateTime.UtcNow;

                state.Tokens = 0;
                state.LastRefillUtc = now;
                state.CooldownUntilUtc = now + _quotaCooldown;
            }
        }

        private bool TryReserve(DateTime now, out string apiKey, out TimeSpan shortestWait)
        {
            apiKey = string.Empty;
            shortestWait = TimeSpan.MaxValue;

            for (var offset = 0; offset < _keys.Length; offset++)
            {
                var index = (_nextIndex + offset) % _keys.Length;
                var state = _keys[index];

                if (now < state.CooldownUntilUtc)
                {
                    TrackWait(state.CooldownUntilUtc - now, ref shortestWait);
                    continue;
                }

                Refill(state, now);

                if (state.Tokens >= 1d)
                {
                    state.Tokens -= 1d;
                    _nextIndex = (index + 1) % _keys.Length;
                    apiKey = state.ApiKey;
                    return true;
                }

                TrackWait(TimeSpan.FromSeconds((1d - state.Tokens) / _refillPerSecond), ref shortestWait);
            }

            shortestWait = shortestWait == TimeSpan.MaxValue
                ? TimeSpan.FromMilliseconds(50)
                : TimeSpan.FromMilliseconds(Math.Clamp(shortestWait.TotalMilliseconds, 10, 1000));

            return false;
        }

        private void Refill(KeyState state, DateTime now)
        {
            var elapsedSeconds = (now - state.LastRefillUtc).TotalSeconds;

            if (elapsedSeconds <= 0)
            {
                return;
            }

            state.Tokens = Math.Min(_capacity, state.Tokens + (elapsedSeconds * _refillPerSecond));
            state.LastRefillUtc = now;
        }

        private static void TrackWait(TimeSpan candidate, ref TimeSpan shortestWait)
        {
            if (candidate < shortestWait)
            {
                shortestWait = candidate;
            }
        }

        private sealed class KeyState
        {
            public KeyState(string apiKey, double tokens, DateTime lastRefillUtc)
            {
                ApiKey = apiKey;
                Tokens = tokens;
                LastRefillUtc = lastRefillUtc;
            }

            public string ApiKey { get; }

            public double Tokens { get; set; }

            public DateTime LastRefillUtc { get; set; }

            public DateTime CooldownUntilUtc { get; set; }
        }
    }
}
