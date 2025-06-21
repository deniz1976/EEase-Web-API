using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest
{
    public class RespondToFriendRequestCommandResponse
    {
        public Header? Header { get; set; }
        public RespondToFriendRequestCommandResponseBody? Body { get; set; }
    }

    public class RespondToFriendRequestCommandResponseBody
    {
        public string Message { get; set; } = "Friend request response processed successfully.";
    }
} 