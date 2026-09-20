using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRouteById
{
    public class GetRouteByIdQueryRequest : IRequest<GetRouteByIdQueryResponse>
    {
        public string Username { get; set; } = string.Empty;
        public Guid? RouteId { get; set; }
    }
}
