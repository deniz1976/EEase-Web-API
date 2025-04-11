using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserCurrency
{
    public class GetUserCurrencyQueryRequest : IRequest<GetUserCurrencyQueryResponse>
    {
        public string? Username { get; set; }
    }
} 