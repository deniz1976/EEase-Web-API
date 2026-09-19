using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions
{
    /// <summary>
    /// The caller is signed in, but this particular thing is not theirs to see or change.
    /// Distinct from <see cref="UnauthorizedAccessException"/>, which means no valid
    /// identity was presented at all.
    /// </summary>
    public sealed class ForbiddenException : BaseException
    {
        public ForbiddenException(string message, StatusEnum statusEnum)
            : base(message, (int)statusEnum)
        {
        }
    }
}
