using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Currency;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using EEaseWebAPI.Persistence.Services.Caching;

namespace EEaseWebAPI.Persistence.Services.ReferenceData
{
    public sealed class CurrencyService : ICurrencyService
    {
        private readonly EEaseAPIDbContext _context;
        private readonly ReferenceDataCache _cache;

        public CurrencyService(EEaseAPIDbContext context, ReferenceDataCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public Task<List<AllWorldCurrencies>> GetCurrenciesAsync() =>
            _cache.GetOrLoadAsync(
                _cache.Keys.AllCurrenciesCacheKey,
                () => _context.Currencies.ToListAsync());

        public Task InitializeCacheAsync() => GetCurrenciesAsync();
    }
}
