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
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        public SendFriendRequestCommandHandler(IFriendshipService friendshipService, IHeaderService headerService)
        {
            _friendshipService = friendshipService;
            _headerService = headerService;
        }

        public async Task<SendFriendRequestCommandResponse> Handle(SendFriendRequestCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _friendshipService.SendRequestAsync(request.RequesterUsername, request.AddresseeUsername);

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
            catch (FriendshipException ex)
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
