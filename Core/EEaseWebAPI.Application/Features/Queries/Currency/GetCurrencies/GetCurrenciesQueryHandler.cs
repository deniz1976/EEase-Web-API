using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Common.Models.Pagination;
using EEaseWebAPI.Application.Enums;
using MediatR;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Currency.GetCurrencies
{
    public class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQueryRequest, GetCurrenciesQueryResponse>
    {
        private readonly ICurrencyService _currencyService;
        private readonly IHeaderService _headerService;
        private readonly IMemoryCache _cache;

        public GetCurrenciesQueryHandler(ICurrencyService currenciesService, IHeaderService headerService, IMemoryCache cache)
        {
            _currencyService = currenciesService;
            _headerService = headerService;
            _cache = cache;
        }

        public async Task<GetCurrenciesQueryResponse> Handle(GetCurrenciesQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.username == null)
                throw new ArgumentNullException(nameof(request));

            var currencies = await GetCurrenciesFromCache();
            
            // Pagination işlemi
            var paginatedCurrencies = currencies
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            var paginatedResult = new PaginatedList<Domain.Entities.Currency.AllWordCurrencies>(
                paginatedCurrencies,
                currencies.Count,
                request.PageNumber,
                request.PageSize);

            return new GetCurrenciesQueryResponse
            {
                getCurrencies = new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.GetCurrenciesSuccessfully),
                    Body = new()
                    {
                        Currencies = paginatedResult
                    }
                }
            };
        }

        private async Task<List<Domain.Entities.Currency.AllWordCurrencies>> GetCurrenciesFromCache()
        {
            const string cacheKey = "AllCurrencies";
            
            if (!_cache.TryGetValue(cacheKey, out List<Domain.Entities.Currency.AllWordCurrencies> currencies))
            {
                currencies = await _currencyService.GetCurrenciesAsync();
                
                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1))
                    .SetAbsoluteExpiration(TimeSpan.FromHours(24));
                
                _cache.Set(cacheKey, currencies, cacheOptions);
            }
            
            return currencies;
        }
    }
}
