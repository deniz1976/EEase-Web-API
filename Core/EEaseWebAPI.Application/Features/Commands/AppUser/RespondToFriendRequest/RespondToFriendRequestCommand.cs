using EEaseWebAPI.Domain.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest
{
    public class RespondToFriendRequestCommand : IRequest<RespondToFriendRequestCommandResponse>
    {
        public string RequesterUsername { get; set; }
        public string AddresseeUsername { get; set; }
        public FriendshipStatus Response { get; set; }
    }
} 