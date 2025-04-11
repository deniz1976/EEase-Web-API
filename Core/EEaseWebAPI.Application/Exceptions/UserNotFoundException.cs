using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions
{
    public class UserNotFoundException : BaseException
    {
        public UserNotFoundException() : base("User not found.", (int)StatusEnum.UserNotFound)
        {
        }

        public UserNotFoundException(string message) : base(message, (int)StatusEnum.UserNotFound)
        {
        }

        public UserNotFoundException(string message, Exception innerException) : base(message, (int)StatusEnum.UserNotFound, innerException)
        {
        }
    }
} 