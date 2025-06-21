using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserFriends
{
    public class GetUserFriendsQueryResponse
    {
        public Header? Header { get; set; }
        public GetUserFriendsQueryResponseBody? Body { get; set; }
    }

    public class GetUserFriendsQueryResponseBody
    {
        public List<UserFriendDto> Friends { get; set; } = new();
    }

    public class UserFriendDto
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }

        public string PhotoPath { get; set; }
        public DateTime FriendshipDate { get; set; }
    }
} 