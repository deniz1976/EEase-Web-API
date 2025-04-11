using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetPendingFriendRequests
{
    public class GetPendingFriendRequestsQueryResponse
    {
        public Header? Header { get; set; }
        public GetPendingFriendRequestsQueryResponseBody? Body { get; set; }
    }

    public class GetPendingFriendRequestsQueryResponseBody
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