using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend
{
    public class RemoveFriendCommandResponse : ApiResponse<RemoveFriendCommandResponseBody>
    {
    }

    public class RemoveFriendCommandResponseBody
    {
        public string? Message { get; set; }
    }
}
