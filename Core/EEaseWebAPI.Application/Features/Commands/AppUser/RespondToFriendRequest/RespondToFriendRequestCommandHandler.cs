using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest
{
    public class RespondToFriendRequestCommandHandler : IRequestHandler<RespondToFriendRequestCommand, RespondToFriendRequestCommandResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        public RespondToFriendRequestCommandHandler(IFriendshipService friendshipService, IHeaderService headerService)
        {
            _friendshipService = friendshipService;
            _headerService = headerService;
        }

        public async Task<RespondToFriendRequestCommandResponse> Handle(RespondToFriendRequestCommand request, CancellationToken cancellationToken)
        {
            try
            {
                await _friendshipService.RespondToRequestAsync(
                    request.RequesterUsername, request.AddresseeUsername, request.Response);

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
            catch (FriendshipException ex)
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
