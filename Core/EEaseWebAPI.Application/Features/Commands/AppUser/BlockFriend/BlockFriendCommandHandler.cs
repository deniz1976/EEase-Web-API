using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Domain.Entities.Identity;
using MediatR;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend
{
    public class BlockFriendCommandHandler : IRequestHandler<BlockFriendCommand, BlockFriendCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public BlockFriendCommandHandler(IFriendshipService friendshipService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _friendshipService = friendshipService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<BlockFriendCommandResponse> Handle(BlockFriendCommand request, CancellationToken cancellationToken)
        {
            await _friendshipService.BlockAsync(request.Username, request.TargetUsername);

            return new BlockFriendCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedSuccessfully),
                Body = new BlockFriendCommandResponseBody
                {
                    Message = _messages["UserBlocked"]
                }
            };
        }
    }
}
