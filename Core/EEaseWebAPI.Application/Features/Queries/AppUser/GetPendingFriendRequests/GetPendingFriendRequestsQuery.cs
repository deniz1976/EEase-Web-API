using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetPendingFriendRequests
{
    public class GetPendingFriendRequestsQuery : IRequest<GetPendingFriendRequestsQueryResponse>
    {
        public string Username { get; set; }
    }
} 