using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikeRoute
{
    public class LikeRouteCommandRequest : IRequest<LikeRouteCommandResponse>
    {
        public string Username { get; set; } = string.Empty;
        public Guid RouteId { get; set; }
    }
}
