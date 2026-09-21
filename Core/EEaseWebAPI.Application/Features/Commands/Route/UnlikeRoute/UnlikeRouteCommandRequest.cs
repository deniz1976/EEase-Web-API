using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.UnlikeRoute
{
    public class UnlikeRouteCommandRequest : IRequest<UnlikeRouteCommandResponse>
    {
        public Guid RouteId { get; set; }

        public string Username { get; set; } = string.Empty;
    }
}
