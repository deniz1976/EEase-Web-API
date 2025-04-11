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
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public BlockFriendCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<BlockFriendCommandResponse> Handle(BlockFriendCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Username == request.TargetUsername)
                    throw new CannotPerformActionOnSelfException();

                var result = await _userService.BlockUserAsync(request.TargetUsername, request.Username);

                if (result) 
                {
                    return new BlockFriendCommandResponse
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedSuccessfully),
                        Body = new BlockFriendCommandResponseBody()
                    };
                }

                throw new Exception("An unexpected error occured");

                
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
            catch (Exception ex)
            {
                throw new FriendshipException("Failed to block user.", StatusEnum.UserBlockFailed, ex);
            }
        }
    }
} 