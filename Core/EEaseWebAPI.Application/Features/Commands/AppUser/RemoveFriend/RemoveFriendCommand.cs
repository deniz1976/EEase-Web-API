using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend
{
    public class RemoveFriendCommand : IRequest<RemoveFriendCommandResponse>
    {
        public string Username { get; set; }
        public string FriendUsername { get; set; }
    }
} 