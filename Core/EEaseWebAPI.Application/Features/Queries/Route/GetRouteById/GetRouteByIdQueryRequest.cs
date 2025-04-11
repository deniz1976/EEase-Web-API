using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRouteById
{
    public class GetRouteByIdQueryRequest : IRequest<GetRouteByIdQueryResponse>
    {
        public string? Username { get; set; }
        public Guid? RouteId { get; set; }
    }
} 