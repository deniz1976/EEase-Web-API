namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public class RemoveFriendResponse : FriendshipBaseResponse<RemoveFriendBody>
    {
    }

    public class RemoveFriendBody
    {
        public string Message { get; set; }
    }
} 