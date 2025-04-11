using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus
{
    public class UpdateRouteStatusCommandRequest : IRequest<UpdateRouteStatusCommandResponse>
    {
        public Guid RouteId { get; set; }
        public int Status { get; set; }
        public string Username { get; set; }
    }
} 