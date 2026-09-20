using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest
{
    public class RespondToFriendRequestCommandResponse : ApiResponse<RespondToFriendRequestCommandResponseBody>
    {
    }

    public class RespondToFriendRequestCommandResponseBody
    {
        public string? Message { get; set; }
    }
}
