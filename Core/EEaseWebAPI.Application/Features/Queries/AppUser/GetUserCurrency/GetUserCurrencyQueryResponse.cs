using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserCurrency
{
    public class GetUserCurrencyQueryResponse : ApiResponse<GetUserCurrencyBody>
    {
    }

    public class GetUserCurrencyBody
    {
        public string? Currency { get; set; }
    }
}
