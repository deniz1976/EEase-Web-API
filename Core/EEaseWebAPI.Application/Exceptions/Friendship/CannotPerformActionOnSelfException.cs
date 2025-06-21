using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class CannotPerformActionOnSelfException : FriendshipException
    {
        public CannotPerformActionOnSelfException() : base("Cannot perform this action on yourself.", StatusEnum.CannotPerformActionOnSelf)
        {
        }

        public CannotPerformActionOnSelfException(string message) : base(message, StatusEnum.CannotPerformActionOnSelf)
        {
        }

        public CannotPerformActionOnSelfException(string message, Exception innerException) : base(message, StatusEnum.CannotPerformActionOnSelf, innerException)
        {
        }
    }
} 