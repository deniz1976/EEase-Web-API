namespace EEaseWebAPI.Application.MapEntities.Friendship
{
    public class SendFriendRequestResponse : FriendshipBaseResponse<SendFriendRequestBody>
    {
    }

    public class SendFriendRequestBody
    {
        public string Message { get; set; }
    }
} 