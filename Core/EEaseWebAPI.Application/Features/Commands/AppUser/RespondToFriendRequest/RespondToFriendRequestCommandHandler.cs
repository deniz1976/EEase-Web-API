using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using MediatR;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest
{
    public class RespondToFriendRequestCommandHandler : IRequestHandler<RespondToFriendRequestCommand, RespondToFriendRequestCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public RespondToFriendRequestCommandHandler(IFriendshipService friendshipService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _friendshipService = friendshipService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<RespondToFriendRequestCommandResponse> Handle(RespondToFriendRequestCommand request, CancellationToken cancellationToken)
        {
            await _friendshipService.RespondToRequestAsync(
                request.RequesterUsername, request.AddresseeUsername, request.Response);

            var statusCode = request.Response == Domain.Enums.FriendshipStatus.Accepted
                ? StatusEnum.FriendRequestAcceptedSuccessfully
                : StatusEnum.FriendRequestRejectedSuccessfully;

            return new RespondToFriendRequestCommandResponse
            {
                Header = _headerService.HeaderCreate((int)statusCode),
                Body = new RespondToFriendRequestCommandResponseBody
                {
                    Message = _messages["FriendRequestAnswered"]
                }
            };
        }
    }
}
