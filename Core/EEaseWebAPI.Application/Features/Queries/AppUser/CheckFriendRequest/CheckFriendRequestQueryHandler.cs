using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest
{
    public class CheckFriendRequestQueryHandler : IRequestHandler<CheckFriendRequestQueryRequest, CheckFriendRequestQueryResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        public CheckFriendRequestQueryHandler(IFriendshipService friendshipService, IHeaderService headerService)
        {
            _friendshipService = friendshipService;
            _headerService = headerService;
        }

        public async Task<CheckFriendRequestQueryResponse> Handle(CheckFriendRequestQueryRequest request, CancellationToken cancellationToken)
        {
            var status = await _friendshipService.GetRequestStatusAsync(request.Username, request.TargetUsername);
            string message = GetMessageForStatus(status);

            return new CheckFriendRequestQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.FriendRequestCheckedSuccessfully),
                Body = new CheckFriendRequestBody
                {
                    Status = status,
                    Message = message
                }
            };
        }

        private string GetMessageForStatus(FriendRequestStatus status)
        {
            return status switch
            {
                FriendRequestStatus.NoRequest => AppMessages.NoFriendRequest,
                FriendRequestStatus.Requester => AppMessages.FriendRequestPending,
                FriendRequestStatus.Addressee => AppMessages.FriendRequestWaitingForYou,
                FriendRequestStatus.AlreadyFriends => AppMessages.AlreadyFriends,
                FriendRequestStatus.Blocked => AppMessages.UserIsBlocked,
                FriendRequestStatus.Self => AppMessages.FriendRequestSelf,
                _ => AppMessages.UnknownFriendRequestStatus
            };
        }
    }
}
