using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus
{
    public class UpdateRouteStatusCommandResponse : ApiResponse<UpdateRouteStatusCommandResponseBody>
    {
    }

    public class UpdateRouteStatusCommandResponseBody
    {
        public bool IsUpdated { get; set; }
    }
}
