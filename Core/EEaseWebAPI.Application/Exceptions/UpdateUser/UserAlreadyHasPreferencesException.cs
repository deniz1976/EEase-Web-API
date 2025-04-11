using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.UpdateUser
{
    public class UserAlreadyHasPreferencesException : BaseException
    {
        public UserAlreadyHasPreferencesException() : base("User already has preferences. Use reset preferences first.", (int)StatusEnum.UserAlreadyHasPreferences)
        {
        }

        public UserAlreadyHasPreferencesException(string message) : base(message, (int)StatusEnum.UserAlreadyHasPreferences)
        {
        }

        public UserAlreadyHasPreferencesException(string message, Exception innerException) : base(message, (int)StatusEnum.UserAlreadyHasPreferences, innerException)
        {
        }
    }
} 