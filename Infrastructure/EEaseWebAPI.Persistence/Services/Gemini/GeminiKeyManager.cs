using System.Collections.Concurrent;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Options;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence.Services.Gemini
{
    public sealed class GeminiKeyManager : IGeminiKeyManager
    {
        private readonly IReadOnlyList<string> _apiKeys;
        private readonly TimeSpan _minimumInterval;
        private readonly ConcurrentDictionary<string, DateTime> _lastUsedUtc = new();

        public GeminiKeyManager(IOptions<GeminiOptions> options)
        {
            var geminiOptions = options.Value;

            _apiKeys = geminiOptions.ApiKeys
                .Where(key => !string.IsNullOrWhiteSpace(key))
                .ToArray();

            _minimumInterval = TimeSpan.FromMinutes(1) / Math.Max(1, geminiOptions.RequestsPerMinutePerKey);
        }

        public async Task<string> GetAvailableApiKey()
        {
            if (_apiKeys.Count == 0)
            {
                throw new GeminiAPIKeyNotFoundException();
            }

            while (true)
            {
                var now = DateTime.UtcNow;
                var shortestWait = TimeSpan.MaxValue;

                foreach (var key in _apiKeys)
                {
                    if (!_lastUsedUtc.TryGetValue(key, out var lastUsed))
                    {
                        return key;
                    }

                    var elapsed = now - lastUsed;

                    if (elapsed >= _minimumInterval)
                    {
                        return key;
                    }

                    var remaining = _minimumInterval - elapsed;
                    if (remaining < shortestWait)
                    {
                        shortestWait = remaining;
                    }
                }

                await Task.Delay(shortestWait);
            }
        }

        public Task MarkKeyAsUsed(string apiKey)
        {
            _lastUsedUtc[apiKey] = DateTime.UtcNow;
            return Task.CompletedTask;
        }

        public Task ReleaseKey(string apiKey) => Task.CompletedTask;
    }
}
