using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend
{
    public class RemoveFriendCommandResponse
    {
        public Header? Header { get; set; }
        public RemoveFriendCommandResponseBody? Body { get; set; }
    }

    public class RemoveFriendCommandResponseBody
    {
        public string Message { get; set; } = "Friend removed successfully.";
    }
} 