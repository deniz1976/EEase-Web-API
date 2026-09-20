using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using MediatR;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend
{
    public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, RemoveFriendCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public RemoveFriendCommandHandler(IFriendshipService friendshipService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _friendshipService = friendshipService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<RemoveFriendCommandResponse> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
        {
            await _friendshipService.RemoveFriendAsync(request.Username, request.FriendUsername);

            return new RemoveFriendCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.FriendRemovedSuccessfully),
                Body = new RemoveFriendCommandResponseBody
                {
                    Message = _messages["FriendRemoved"]
                }
            };
        }
    }
}
