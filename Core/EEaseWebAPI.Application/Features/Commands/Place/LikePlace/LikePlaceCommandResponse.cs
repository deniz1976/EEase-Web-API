using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.DTOs;

namespace EEaseWebAPI.Application.Features.Commands.Place.LikePlace
{
    public class LikePlaceCommandResponse : ApiResponse<LikePlaceCommandResponseBody>
    {
    }

    public class LikePlaceCommandResponseBody
    {
        public bool IsPreferenceUpdated { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
