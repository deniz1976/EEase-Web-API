using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikeRoute
{
    public class LikeRouteCommandRequest : IRequest<LikeRouteCommandResponse>
    {
        public string? Username { get; set; }
        public Guid RouteId { get; set; }
    }
} 