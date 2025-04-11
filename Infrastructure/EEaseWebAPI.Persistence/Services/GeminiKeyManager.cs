using EEaseWebAPI.Application.Abstractions.Services;
using Microsoft.Extensions.Configuration;
using System.Collections.Concurrent;

namespace EEaseWebAPI.Persistence.Services
{
    /// <summary>
    /// Manages the allocation and rate limiting of Gemini AI API keys.
    /// Provides functionality for key rotation and usage tracking to prevent quota exhaustion.
    /// </summary>
    public class GeminiKeyManager : IGeminiKeyManager
    {
        private readonly IConfiguration _configuration;
        private readonly ConcurrentDictionary<string, DateTime> _keyUsageTimestamps;
        private readonly int _requestsPerMinutePerKey;
        private readonly object _lock = new object();

        /// <summary>
        /// Initializes a new instance of the GeminiKeyManager.
        /// </summary>
        /// <param name="configuration">Application configuration containing API key settings and rate limits</param>
        public GeminiKeyManager(IConfiguration configuration)
        {
            _configuration = configuration;
            _keyUsageTimestamps = new ConcurrentDictionary<string, DateTime>();
            _requestsPerMinutePerKey = _configuration.GetValue<int>("GeminiAI:RequestsPerMinutePerKey");

            var apiKeys = _configuration.GetSection("GeminiAI:ApiKeys").Get<string[]>();
            if (apiKeys != null)
            {
                foreach (var key in apiKeys)
                {
                    _keyUsageTimestamps.TryAdd(key, DateTime.UtcNow.AddMilliseconds(-100));
                }
            }
        }

        /// <summary>
        /// Retrieves an available API key that hasn't exceeded its rate limit.
        /// </summary>
        /// <returns>An available API key</returns>
        /// <exception cref="InvalidOperationException">Thrown when no API keys are configured</exception>
        /// <remarks>
        /// This method will wait until a key becomes available if all keys are currently at their rate limit.
        /// Keys are considered available after a one-minute cooldown period from their last use.
        /// </remarks>
        public async Task<string> GetAvailableApiKey()
        {
            var apiKeys = _configuration.GetSection("GeminiAI:ApiKeys").Get<string[]>();
            if (apiKeys == null || apiKeys.Length == 0)
            {
                throw new InvalidOperationException("No API keys configured");
            }

            while (true)
            {
                foreach (var key in apiKeys)
                {
                    if (IsKeyAvailable(key))
                    {
                        return key;
                    }
                }

                await Task.Delay(100);
            }
        }

        /// <summary>
        /// Marks an API key as used, updating its last usage timestamp.
        /// </summary>
        /// <param name="apiKey">The API key to mark as used</param>
        /// <returns>A completed task</returns>
        public Task MarkKeyAsUsed(string apiKey)
        {
            _keyUsageTimestamps.AddOrUpdate(
                apiKey,
                DateTime.UtcNow,
                (_, _) => DateTime.UtcNow);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Releases an API key after use.
        /// Currently a no-op as key availability is determined by time-based rate limiting.
        /// </summary>
        /// <param name="apiKey">The API key to release</param>
        /// <returns>A completed task</returns>
        public Task ReleaseKey(string apiKey)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Checks if an API key is available for use based on its rate limit.
        /// </summary>
        /// <param name="key">The API key to check</param>
        /// <returns>True if the key is available, false otherwise</returns>
        /// <remarks>
        /// A key is considered available if it hasn't been used in the last second,
        /// allowing for rate limit reset.
        /// </remarks>
        private bool IsKeyAvailable(string key)
        {
            if (_keyUsageTimestamps.TryGetValue(key, out DateTime lastUsed))
            {
                var timeSinceLastUse = DateTime.UtcNow - lastUsed;
                return timeSinceLastUse.TotalMilliseconds >= 100;
            }
            return true;
        }
    }
} 