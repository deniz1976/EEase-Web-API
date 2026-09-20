using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRoutesByUserId
{
    public class GetRoutesByUserIdQueryRequest : IRequest<GetRoutesByUserIdQueryResponse>
    {
        public string UserId { get; set; } = string.Empty;
        public string RequesterUsername { get; set; } = string.Empty;
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}
