namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public class BlockFriendResponse : FriendshipBaseResponse<BlockFriendBody>
    {
    }

    public class BlockFriendBody
    {
        public string Message { get; set; }
    }
} 