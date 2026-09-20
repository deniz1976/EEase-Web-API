using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute
{
    public class DeleteRouteCommandResponse : ApiResponse<DeleteRouteCommandResponseBody>
    {
    }

    public class DeleteRouteCommandResponseBody
    {
        public bool IsDeleted { get; set; }
    }
}
