using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend
{
    public class BlockFriendCommand : IRequest<BlockFriendCommandResponse>
    {
        public string Username { get; set; }
        public string TargetUsername { get; set; }
    }
} 