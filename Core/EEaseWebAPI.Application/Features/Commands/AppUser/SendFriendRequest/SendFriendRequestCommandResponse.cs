using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendFriendRequest
{
    public class SendFriendRequestCommandResponse
    {
        public Header? Header { get; set; }
        public SendFriendRequestCommandResponseBody? Body { get; set; }
    }

    public class SendFriendRequestCommandResponseBody
    {
        public string Message { get; set; } = "Friend request sent successfully.";
    }
} 