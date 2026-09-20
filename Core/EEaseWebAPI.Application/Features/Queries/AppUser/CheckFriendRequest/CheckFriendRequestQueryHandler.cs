using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest
{
    public class CheckFriendRequestQueryHandler : IRequestHandler<CheckFriendRequestQueryRequest, CheckFriendRequestQueryResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public CheckFriendRequestQueryHandler(IFriendshipService friendshipService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _friendshipService = friendshipService;
            _headerService = headerService;

            _messages = messages;
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
                FriendRequestStatus.NoRequest => _messages["NoFriendRequest"],
                FriendRequestStatus.Requester => _messages["FriendRequestPending"],
                FriendRequestStatus.Addressee => _messages["FriendRequestWaitingForYou"],
                FriendRequestStatus.AlreadyFriends => _messages["AlreadyFriends"],
                FriendRequestStatus.Blocked => _messages["UserIsBlocked"],
                _ => _messages["UnknownFriendRequestStatus"]
            };
        }
    }
}
