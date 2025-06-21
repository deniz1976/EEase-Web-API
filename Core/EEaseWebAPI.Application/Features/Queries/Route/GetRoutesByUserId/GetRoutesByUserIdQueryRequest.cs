using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRoutesByUserId
{
    public class GetRoutesByUserIdQueryRequest : IRequest<GetRoutesByUserIdQueryResponse>
    {
        public string UserId { get; set; }
        public string RequesterUsername { get; set; }
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
} 