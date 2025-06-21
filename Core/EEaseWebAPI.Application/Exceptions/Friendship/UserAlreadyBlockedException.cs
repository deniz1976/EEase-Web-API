using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.Friendship
{
    public class UserAlreadyBlockedException : FriendshipException
    {
        public UserAlreadyBlockedException() : base("This user is already blocked.", StatusEnum.UserAlreadyBlocked)
        {
        }

        public UserAlreadyBlockedException(string message) : base(message, StatusEnum.UserAlreadyBlocked)
        {
        }

        public UserAlreadyBlockedException(string message, Exception innerException) : base(message, StatusEnum.UserAlreadyBlocked, innerException)
        {
        }
    }
} 