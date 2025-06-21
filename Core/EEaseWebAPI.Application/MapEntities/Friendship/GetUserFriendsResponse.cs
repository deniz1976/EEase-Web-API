using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public class GetUserFriendsResponse : FriendshipBaseResponse<GetUserFriendsBody>
    {
        public GetUserFriendsResponse()
        {
            Header = new EEaseWebAPI.Application.MapEntities.Header();
            Body = new GetUserFriendsBody();
        }
    }

    public class GetUserFriendsBody
    {
        public List<UserFriendDto> Friends { get; set; } = new();
    }

    public class UserFriendDto
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime FriendshipDate { get; set; }
    }
} 