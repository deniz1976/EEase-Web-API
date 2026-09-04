using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public class GetPendingFriendRequestsResponse : FriendshipBaseResponse<GetPendingFriendRequestsBody>
    {
        public GetPendingFriendRequestsResponse()
        {
            Header = new EEaseWebAPI.Application.MapEntities.Header();
            Body = new GetPendingFriendRequestsBody();
        }
    }

    public class GetPendingFriendRequestsBody
    {
        public List<PendingFriendRequestDto> PendingRequests { get; set; } = new();
    }

    public class PendingFriendRequestDto
    {
        public string RequesterUsername { get; set; }
        public string RequesterName { get; set; }
        public string RequesterSurname { get; set; }
        public DateTime RequestDate { get; set; }
    }
} 