using System.Collections.Concurrent;
using EEaseWebAPI.Application.Options;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence.Services.Caching
{
    public sealed class ReferenceDataCache
    {
        // The cache itself is a singleton, so this has to be one too: the point is that two
        // requests that arrive with the entry cold share one read, and they only share it if
        // they are looking at the same dictionary.
        private static readonly ConcurrentDictionary<string, Task> Loading = new();

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
            if (TryRead(key, out T? cached))
            {
                return cached!;
            }

            // Every city in the world is one row at a time out of the database. Without this,
            // a cold cache under load reads the whole table once per request in flight.
            var pending = (Task<T>)Loading.GetOrAdd(key, _ => LoadAsync(key, load, cancellationToken));

            try
            {
                return await pending;
            }
            finally
            {
                Loading.TryRemove(key, out _);
            }
        }

        public bool IsLoaded(string key) => _memoryCache.TryGetValue(key, out _);

        private async Task<T> LoadAsync<T>(
            string key, Func<CancellationToken, Task<T>> load, CancellationToken cancellationToken)
        {
            // The read that got here first may have finished while this one was queued.
            if (TryRead(key, out T? cached))
            {
                return cached!;
            }

            var loaded = await load(cancellationToken);

            _memoryCache.Set(key, loaded, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(_options.ReferenceDataLifetimeHours)));

            return loaded;
        }

        private bool TryRead<T>(string key, out T? value)
        {
            value = default;

            return _memoryCache.TryGetValue(key, out T? cached) && cached is not null && (value = cached) is not null;
        }
    }
}
