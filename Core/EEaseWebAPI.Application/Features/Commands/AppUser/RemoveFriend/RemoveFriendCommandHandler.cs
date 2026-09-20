using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Application.Resources;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend
{
    public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, RemoveFriendCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        public RemoveFriendCommandHandler(IFriendshipService friendshipService, IHeaderService headerService)
        {
            _friendshipService = friendshipService;
            _headerService = headerService;
        }

        public async Task<RemoveFriendCommandResponse> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
        {
            await _friendshipService.RemoveFriendAsync(request.Username, request.FriendUsername, cancellationToken);

            return new RemoveFriendCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.FriendRemovedSuccessfully),
                Body = new RemoveFriendCommandResponseBody
                {
                    Message = AppMessages.FriendRemoved
                }
            };
        }
    }
}
