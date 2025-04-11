using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute
{
    public class DeleteRouteCommandResponse
    {
        public Header? Header { get; set; }
        public DeleteRouteCommandResponseBody? Body { get; set; }
    }

    public class DeleteRouteCommandResponseBody
    {
        public bool IsDeleted { get; set; }
    }
} 