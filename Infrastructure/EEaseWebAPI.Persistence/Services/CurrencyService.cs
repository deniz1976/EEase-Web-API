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
    public class CurrencyService : ICurrencyService
    {
        private readonly EEaseAPIDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly string _currenciesCacheKey;

        public CurrencyService(EEaseAPIDbContext context, IMemoryCache memoryCache, IConfiguration configuration) 
        {
            _context = context;
            _memoryCache = memoryCache;
            _currenciesCacheKey = configuration["CacheConfiguration:AllCurrenciesCacheKey"] ?? "AllCurrencies_Cache";
        }

        public async Task<List<AllWorldCurrencies>> GetCurrenciesAsync() 
        {
            if (_memoryCache.TryGetValue(_currenciesCacheKey, out List<AllWorldCurrencies> cachedCurrencies))
            {
                return cachedCurrencies;
            }

            var currencies = await _context.Currencies.ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(24));

            _memoryCache.Set(_currenciesCacheKey, currencies, cacheOptions);

            return currencies;
        }

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
