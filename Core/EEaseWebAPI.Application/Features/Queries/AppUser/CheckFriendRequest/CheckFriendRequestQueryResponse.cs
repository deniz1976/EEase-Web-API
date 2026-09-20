using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.GetAccountStatus;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest
{
    public class CheckFriendRequestQueryResponse : ApiResponse<CheckFriendRequestBody>
    {
    }

    public class CheckFriendRequestBody
    {
        public FriendRequestStatus? Status { get; set; }
        public string? Message { get; set; }
    }
}
