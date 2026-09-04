using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
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
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public CheckFriendRequestQueryHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<CheckFriendRequestQueryResponse> Handle(CheckFriendRequestQueryRequest request, CancellationToken cancellationToken)
        {
            var status = await _userService.CheckFriendRequest(request.Username, request.TargetUsername);
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
                FriendRequestStatus.NoRequest => "No friend request exists yet.",
                FriendRequestStatus.Requester => "Your friend request is pending.",
                FriendRequestStatus.Addressee => "You have a friend request.",
                FriendRequestStatus.AlreadyFriends => "You are already friends with this user.",
                FriendRequestStatus.Blocked => "This user is blocked.",
                _ => "Unknown status."
            };
        }
    }
}
