using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.StatusCheck;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest
{
    public class CheckFriendRequestQueryResponse
    {
        public CheckFriendRequestBody? Body { get; set; }
        public Header? Header { get; set; }
    }

    public class CheckFriendRequestBody
    {
        public FriendRequestStatus? Status { get; set; }
        public string? Message { get; set; }
    }
}
