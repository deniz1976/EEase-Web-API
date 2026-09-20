using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend
{
    public class BlockFriendCommandResponse : ApiResponse<BlockFriendCommandResponseBody>
    {
    }

    public class BlockFriendCommandResponseBody
    {
        public string? Message { get; set; }
    }
}
