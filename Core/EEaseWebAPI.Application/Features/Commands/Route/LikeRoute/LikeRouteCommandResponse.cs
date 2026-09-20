using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikeRoute
{
    public class LikeRouteCommandResponse : ApiResponse<LikeRouteCommandResponseBody>
    {
    }

    public class LikeRouteCommandResponseBody
    {
        public bool IsLiked { get; set; }
    }
}
