using EEaseWebAPI.Application.Common.Models.Pagination;
using EEaseWebAPI.Domain.Entities.Currency;

namespace EEaseWebAPI.Application.MapEntities.GetCurrencies
{
    public class GetCurrenciesBody
    {
        public PaginatedList<AllWordCurrencies>? Currencies { get; set; }
    }
} 