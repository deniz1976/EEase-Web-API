using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.ResetUserPreferences
{
    public class UserPreferencesNotFoundException : BaseException
    {
        public UserPreferencesNotFoundException() : base("User not found while trying to reset preferences.", (int)StatusEnum.UserNotFound)
        {
        }

        public UserPreferencesNotFoundException(string message) : base(message, (int)StatusEnum.UserNotFound)
        {
        }

        public UserPreferencesNotFoundException(string message, Exception innerException) : base(message, (int)StatusEnum.UserNotFound, innerException)
        {
        }
    }
} 