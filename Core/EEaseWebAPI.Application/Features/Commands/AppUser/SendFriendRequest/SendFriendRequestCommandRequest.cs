using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendFriendRequest
{
    public class SendFriendRequestCommandRequest : IRequest<SendFriendRequestCommandResponse>
    {
        public string? RequesterUsername { get; set; }
        public string? AddresseeUsername { get; set; }
    }
} 