using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus
{
    public class UpdateRouteStatusCommandResponse
    {
        public Header? Header { get; set; }
        public UpdateRouteStatusCommandResponseBody? Body { get; set; }
    }

    public class UpdateRouteStatusCommandResponseBody
    {
        public bool IsUpdated { get; set; }
    }
} 