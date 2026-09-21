using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions
{
    public sealed class ForbiddenException : BaseException
    {
        public ForbiddenException(string message, StatusEnum statusEnum)
            : base(message, (int)statusEnum)
        {
        }
    }
}
