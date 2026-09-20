using EEaseWebAPI.Application.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence.Services.Caching
{
    /// <summary>
    /// Cities, countries and currencies change about once a year, so they are read once and
    /// kept in memory for as long as the configuration says.
    /// </summary>
    public sealed class ReferenceDataCache
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CacheOptions _options;

        public ReferenceDataCache(IMemoryCache memoryCache, IOptions<CacheOptions> options)
        {
            _memoryCache = memoryCache;
            _options = options.Value;
        }

        public CacheOptions Keys => _options;

        public async Task<T> GetOrLoadAsync<T>(
            string key, Func<CancellationToken, Task<T>> load, CancellationToken cancellationToken = default)
        {
            if (_memoryCache.TryGetValue(key, out T? cached) && cached is not null)
            {
                return cached;
            }

            var loaded = await load(cancellationToken);

            _memoryCache.Set(key, loaded, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(_options.ReferenceDataLifetimeHours)));

            return loaded;
        }

        public bool IsLoaded(string key) => _memoryCache.TryGetValue(key, out _);
    }
}
