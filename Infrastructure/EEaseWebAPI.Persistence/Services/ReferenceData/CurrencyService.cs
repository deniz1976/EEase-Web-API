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

        public Task<List<AllWorldCurrencies>> GetCurrenciesAsync(CancellationToken cancellationToken = default) =>
            _cache.GetOrLoadAsync(
                _cache.Keys.AllCurrenciesCacheKey,
                token => _context.Currencies.ToListAsync(token),
                cancellationToken);

        public Task InitializeCacheAsync(CancellationToken cancellationToken = default) =>
            GetCurrenciesAsync(cancellationToken);
    }
}
