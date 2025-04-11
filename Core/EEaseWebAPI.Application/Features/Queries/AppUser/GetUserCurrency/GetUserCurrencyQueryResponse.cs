

using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserCurrency
{
    public class GetUserCurrencyQueryResponse
    {
        public Header? Header { get; set; }
        public GetUserCurrencyBody? Body { get; set; }
    }



    public class GetUserCurrencyBody
    {
        public string? Currency { get; set; }
    }
} 