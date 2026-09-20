using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Common.Models.Pagination;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Currency.GetCurrencies
{
    public class GetCurrenciesQueryHandler : IRequestHandler<GetCurrenciesQueryRequest, GetCurrenciesQueryResponse>
    {
        private readonly ICurrencyService _currencyService;
        private readonly IHeaderService _headerService;

        public GetCurrenciesQueryHandler(ICurrencyService currencyService, IHeaderService headerService)
        {
            _currencyService = currencyService;
            _headerService = headerService;
        }

        public async Task<GetCurrenciesQueryResponse> Handle(
            GetCurrenciesQueryRequest request, CancellationToken cancellationToken)
        {
            // The service already keeps the list in memory. Caching it again here, under a
            // second key with a lifetime of its own, only made it possible for the two
            // copies to disagree.
            var currencies = await _currencyService.GetCurrenciesAsync();

            var page = currencies
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new GetCurrenciesQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetCurrenciesSuccessfully),
                Body = new()
                {
                    Currencies = new PaginatedList<Domain.Entities.Currency.AllWorldCurrencies>(
                        page, currencies.Count, request.PageNumber, request.PageSize)
                }
            };
        }
    }
}
