using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend
{
    public class BlockFriendCommandResponse
    {
        public Header? Header { get; set; }
        public BlockFriendCommandResponseBody? Body { get; set; }
    }

    public class BlockFriendCommandResponseBody
    {
        public string Message { get; set; } = "User blocked successfully.";
    }
} 