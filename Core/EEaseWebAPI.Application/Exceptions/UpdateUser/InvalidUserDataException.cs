using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.UpdateUser
{
    public sealed class InvalidUserDataException : BaseException
    {
        public InvalidUserDataException(string message)
            : base(message, (int)StatusEnum.InvalidUserData) { }
    }
}
