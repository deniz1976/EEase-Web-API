using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserFriends
{
    public class GetUserFriendsQuery : IRequest<GetUserFriendsQueryResponse>
    {
        public string Username { get; set; }
    }
} 