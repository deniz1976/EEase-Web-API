using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public class GetBlockedUsersResponse : FriendshipBaseResponse<GetBlockedUsersBody>
    {
        public GetBlockedUsersResponse()
        {
            Header = new EEaseWebAPI.Application.MapEntities.Header();
            Body = new GetBlockedUsersBody();
        }
    }

    public class GetBlockedUsersBody
    {
        public List<BlockedUserDto> BlockedUsers { get; set; } = new();
    }

    public class BlockedUserDto
    {
        public string Username { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public DateTime BlockedDate { get; set; }
    }
} 