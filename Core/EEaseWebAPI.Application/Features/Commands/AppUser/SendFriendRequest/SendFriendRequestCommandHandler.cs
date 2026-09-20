using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Domain.Entities.Identity;
using MediatR;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendFriendRequest
{
    public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommandRequest, SendFriendRequestCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public SendFriendRequestCommandHandler(IFriendshipService friendshipService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _friendshipService = friendshipService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<SendFriendRequestCommandResponse> Handle(SendFriendRequestCommandRequest request, CancellationToken cancellationToken)
        {
            await _friendshipService.SendRequestAsync(request.RequesterUsername, request.AddresseeUsername);

            return new SendFriendRequestCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.FriendRequestSentSuccessfully),
                Body = new SendFriendRequestCommandResponseBody
                {
                    Message = _messages["FriendRequestSent"]
                }
            };
        }
    }
}
