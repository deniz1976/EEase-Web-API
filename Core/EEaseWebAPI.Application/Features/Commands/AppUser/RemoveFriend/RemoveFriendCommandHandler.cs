using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend
{
    public class RemoveFriendCommandHandler : IRequestHandler<RemoveFriendCommand, RemoveFriendCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public RemoveFriendCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<RemoveFriendCommandResponse> Handle(RemoveFriendCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.Username == request.FriendUsername)
                    throw new CannotPerformActionOnSelfException();

                var friendship = await _userService.GetFriendshipAsync(request.Username, request.FriendUsername);
                if (friendship == null)
                    throw new FriendshipNotFoundException();

                bool result = await _userService.RemoveFriendshipAsync(friendship);

                return new RemoveFriendCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.FriendRemovedSuccessfully),
                    Body = new RemoveFriendCommandResponseBody()
                };
            }
            catch (UserNotFoundException ex)
            {
                throw;
            }
            catch (FriendshipNotFoundException ex)
            {
                throw;
            }
            catch (CannotPerformActionOnSelfException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FriendshipException("Failed to remove friend.", StatusEnum.FriendRemovalFailed, ex);
            }
        }
    }
} 