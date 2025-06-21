using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class InvalidFriendshipStatusException : FriendshipException
    {
        public InvalidFriendshipStatusException() : base("Invalid friendship status.", StatusEnum.InvalidFriendshipStatus)
        {
        }

        public InvalidFriendshipStatusException(string message) : base(message, StatusEnum.InvalidFriendshipStatus)
        {
        }

        public InvalidFriendshipStatusException(string message, Exception innerException) : base(message, StatusEnum.InvalidFriendshipStatus, innerException)
        {
        }
    }
} 