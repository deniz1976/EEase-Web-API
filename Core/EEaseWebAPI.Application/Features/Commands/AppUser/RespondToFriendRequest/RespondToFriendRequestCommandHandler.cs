using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest
{
    public class RespondToFriendRequestCommandHandler : IRequestHandler<RespondToFriendRequestCommand, RespondToFriendRequestCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;
        private readonly UserManager<Domain.Entities.Identity.AppUser> _userManager;

        public RespondToFriendRequestCommandHandler(IUserService userService, IHeaderService headerService,UserManager<Domain.Entities.Identity.AppUser> userManager)
        {
            _userService = userService;
            _headerService = headerService;
            _userManager = userManager;
        }

        public async Task<RespondToFriendRequestCommandResponse> Handle(RespondToFriendRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var requester = await _userManager.FindByNameAsync(request.RequesterUsername);
                var addressee = await _userManager.FindByNameAsync(request.AddresseeUsername);

                if(requester == null || addressee == null) 
                {
                    throw new FriendshipNotFoundException();
                }

                var friendship = await _userService.GetFriendshipAsync(request.RequesterUsername, request.AddresseeUsername);

                if (friendship == null)
                    throw new FriendshipNotFoundException();

                if (friendship.AddresseeId != addressee.Id)
                    throw new FriendshipException("Only the recipient of the friend request can respond to it.", StatusEnum.FriendRequestResponseFailed);

                if (friendship.Status != Domain.Enums.FriendshipStatus.Pending)
                    throw new FriendshipException("Can only respond to pending friend requests.", StatusEnum.FriendRequestResponseFailed);

                bool result = await _userService.UpdateFriendshipStatusAsync(request.RequesterUsername, request.AddresseeUsername, request.Response);

                var statusCode = request.Response == Domain.Enums.FriendshipStatus.Accepted
                    ? StatusEnum.FriendRequestAcceptedSuccessfully
                    : StatusEnum.FriendRequestRejectedSuccessfully;

                return new RespondToFriendRequestCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)statusCode),
                    Body = new RespondToFriendRequestCommandResponseBody()
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
            catch (Exception ex)
            {
                throw new FriendshipException("Failed to process friend request response.", StatusEnum.FriendRequestResponseFailed, ex);
            }
        }
    }
} 