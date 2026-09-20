using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendFriendRequest
{
    public class SendFriendRequestCommandResponse : ApiResponse<SendFriendRequestCommandResponseBody>
    {
    }

    public class SendFriendRequestCommandResponseBody
    {
        public string? Message { get; set; }
    }
}
