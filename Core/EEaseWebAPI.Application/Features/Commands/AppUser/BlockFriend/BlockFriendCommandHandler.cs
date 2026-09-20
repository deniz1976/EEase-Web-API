using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Application.Resources;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend
{
    public class BlockFriendCommandHandler : IRequestHandler<BlockFriendCommand, BlockFriendCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        public BlockFriendCommandHandler(IFriendshipService friendshipService, IHeaderService headerService)
        {
            _friendshipService = friendshipService;
            _headerService = headerService;
        }

        public async Task<BlockFriendCommandResponse> Handle(BlockFriendCommand request, CancellationToken cancellationToken)
        {
            await _friendshipService.BlockAsync(request.Username, request.TargetUsername);

            return new BlockFriendCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedSuccessfully),
                Body = new BlockFriendCommandResponseBody
                {
                    Message = AppMessages.UserBlocked
                }
            };
        }
    }
}
