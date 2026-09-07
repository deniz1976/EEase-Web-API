using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Domain.Entities.Identity;
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
            try
            {
                await _friendshipService.BlockAsync(request.Username, request.TargetUsername);

                return new BlockFriendCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedSuccessfully),
                    Body = new BlockFriendCommandResponseBody()
                };
            }
            catch (UserNotFoundException)
            {
                throw;
            }
            catch (CannotPerformActionOnSelfException)
            {
                throw;
            }
            catch (UserAlreadyBlockedException)
            {
                throw;
            }
            catch (FriendshipException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FriendshipException("Failed to block user.", StatusEnum.UserBlockFailed, ex);
            }
        }
    }
}
