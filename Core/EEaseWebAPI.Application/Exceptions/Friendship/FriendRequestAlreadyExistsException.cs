using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class FriendRequestAlreadyExistsException : FriendshipException
    {
        public FriendRequestAlreadyExistsException() : base("A friendship request already exists between these users.", StatusEnum.FriendRequestAlreadyExists)
        {
        }

        public FriendRequestAlreadyExistsException(string message) : base(message, StatusEnum.FriendRequestAlreadyExists)
        {
        }

        public FriendRequestAlreadyExistsException(string message, Exception innerException) : base(message, StatusEnum.FriendRequestAlreadyExists, innerException)
        {
        }
    }
} 