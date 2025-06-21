using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikeRoute
{
    public class LikeRouteCommandResponse
    {
        public Header? Header { get; set; }
        public LikeRouteCommandResponseBody? Body { get; set; }
    }

    public class LikeRouteCommandResponseBody
    {
        public bool IsLiked { get; set; }
    }
} 