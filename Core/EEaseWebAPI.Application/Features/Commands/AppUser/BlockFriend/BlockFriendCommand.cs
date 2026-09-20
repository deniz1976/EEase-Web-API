using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend
{
    public class BlockFriendCommand : IRequest<BlockFriendCommandResponse>
    {
        public string Username { get; set; } = string.Empty;
        public string TargetUsername { get; set; } = string.Empty;
    }
}
