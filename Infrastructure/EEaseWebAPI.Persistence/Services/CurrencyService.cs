using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Currency;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Services
{
    /// <summary>
    /// Service responsible for managing currency operations.
    /// Handles data management and caching for world currencies.
    /// </summary>
    public class CurrencyService : ICurrencyService
    {
        private readonly EEaseAPIDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly string _currenciesCacheKey;

        /// <summary>
        /// Initializes a new instance of the CurrencyService.
        /// </summary>
        /// <param name="context">Database context for accessing currency data</param>
        /// <param name="memoryCache">Memory cache service for caching currency data</param>
        /// <param name="configuration">Application configuration for cache settings</param>
        public CurrencyService(EEaseAPIDbContext context, IMemoryCache memoryCache, IConfiguration configuration) 
        {
            _context = context;
            _memoryCache = memoryCache;
            _currenciesCacheKey = configuration["CacheConfiguration:AllCurrenciesCacheKey"] ?? "AllCurrencies_Cache";
        }

        /// <summary>
        /// Retrieves all currencies from the database or cache.
        /// </summary>
        /// <returns>A list of world currencies</returns>
        /// <remarks>
        /// Returns data from cache if available, otherwise fetches from database and caches it.
        /// Cache duration is set to 24 hours sliding expiration.
        /// </remarks>
        public async Task<List<AllWordCurrencies>> GetCurrenciesAsync() 
        {
            if (_memoryCache.TryGetValue(_currenciesCacheKey, out List<AllWordCurrencies> cachedCurrencies))
            {
                return cachedCurrencies;
            }

            var currencies = await _context.Currencies.ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(24));

            _memoryCache.Set(_currenciesCacheKey, currencies, cacheOptions);

            return currencies;
        }

        /// <summary>
        /// Initializes the cache with currency data at application startup.
        /// </summary>
        /// <remarks>
        /// This method is called during application startup to ensure currency data
        /// is readily available in the cache for fast access.
        /// </remarks>
        public async Task InitializeCacheAsync()
        {
            if (!_memoryCache.TryGetValue(_currenciesCacheKey, out _))
            {
                var currencies = await _context.Currencies.ToListAsync();
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24));

                _memoryCache.Set(_currenciesCacheKey, currencies, cacheOptions);
            }
        }
    }
}
