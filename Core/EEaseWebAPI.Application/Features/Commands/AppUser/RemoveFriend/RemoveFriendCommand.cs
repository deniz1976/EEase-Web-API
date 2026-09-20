using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend
{
    public class RemoveFriendCommand : IRequest<RemoveFriendCommandResponse>
    {
        public string Username { get; set; } = string.Empty;
        public string FriendUsername { get; set; } = string.Empty;
    }
}
