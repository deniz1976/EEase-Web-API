using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute
{
    public class DeleteRouteCommandRequest : IRequest<DeleteRouteCommandResponse>
    {
        public string Username { get; set; } = string.Empty;
        public Guid RouteId { get; set; }
    }
}
