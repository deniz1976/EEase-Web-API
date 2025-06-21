using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Domain.Entities.Identity;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendFriendRequest
{
    public class SendFriendRequestCommandHandler : IRequestHandler<SendFriendRequestCommandRequest, SendFriendRequestCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public SendFriendRequestCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<SendFriendRequestCommandResponse> Handle(SendFriendRequestCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                if (request.RequesterUsername == request.AddresseeUsername)
                    throw new CannotPerformActionOnSelfException();

                var friendship = await _userService.GetFriendshipAsync(request.RequesterUsername, request.AddresseeUsername);

                if (friendship != null)
                {
                    if (friendship.Status == Domain.Enums.FriendshipStatus.Blocked && 
                        friendship.RequesterId != request.RequesterUsername)
                        throw new UserBlockedException("You cannot send a friend request to this user.");

                    if (friendship.Status == Domain.Enums.FriendshipStatus.Accepted)
                        throw new FriendshipException("You are already friends with this user.", StatusEnum.FriendRequestSendFailed);

                    throw new FriendRequestAlreadyExistsException();
                }

                bool result = await _userService.CreateFriendshipAsync(request.RequesterUsername,request.AddresseeUsername);

                return new SendFriendRequestCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.FriendRequestSentSuccessfully),
                    Body = new SendFriendRequestCommandResponseBody()
                };
            }
            catch (CannotPerformActionOnSelfException ex)
            {
                throw;
            }
            catch (FriendRequestAlreadyExistsException ex)
            {
                throw;
            }
            catch (UserBlockedException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new FriendshipException("Failed to send friend request.", StatusEnum.FriendRequestSendFailed, ex);
            }
        }
    }
} 