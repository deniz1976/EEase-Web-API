using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class FriendshipException : BaseException
    {
        public FriendshipException(string message, StatusEnum statusEnum) : base(message, (int)statusEnum)
        {
        }

        public FriendshipException(string message, StatusEnum statusEnum, Exception innerException) : base(message, (int)statusEnum, innerException)
        {
        }
    }
} 