using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.ResetUserPreferences
{
    public class ResetUserPreferencesFailedException : BaseException
    {
        public ResetUserPreferencesFailedException() : base("Failed to reset user preferences.", (int)StatusEnum.PreferencesResetFailed)
        {
        }

        public ResetUserPreferencesFailedException(string message) : base(message, (int)StatusEnum.PreferencesResetFailed)
        {
        }

        public ResetUserPreferencesFailedException(string message, Exception innerException) : base(message, (int)StatusEnum.PreferencesResetFailed, innerException)
        {
        }
    }
} 