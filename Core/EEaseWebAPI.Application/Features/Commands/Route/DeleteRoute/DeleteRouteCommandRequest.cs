using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute
{
    public class DeleteRouteCommandRequest : IRequest<DeleteRouteCommandResponse>
    {
        public string? Username { get; set; }
        public Guid RouteId { get; set; }
    }
} 