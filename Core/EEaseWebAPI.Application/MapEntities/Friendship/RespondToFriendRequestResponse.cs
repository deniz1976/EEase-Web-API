namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public class RespondToFriendRequestResponse : FriendshipBaseResponse<RespondToFriendRequestBody>
    {
    }

    public class RespondToFriendRequestBody
    {
        public string Message { get; set; }
    }
} 