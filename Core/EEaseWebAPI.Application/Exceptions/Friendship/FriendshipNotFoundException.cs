using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class FriendshipNotFoundException : FriendshipException
    {
        public FriendshipNotFoundException() : base("No friendship exists between these users.", StatusEnum.FriendshipNotFound)
        {
        }

        public FriendshipNotFoundException(string message) : base(message, StatusEnum.FriendshipNotFound)
        {
        }

        public FriendshipNotFoundException(string message, Exception innerException) : base(message, StatusEnum.FriendshipNotFound, innerException)
        {
        }
    }
} 