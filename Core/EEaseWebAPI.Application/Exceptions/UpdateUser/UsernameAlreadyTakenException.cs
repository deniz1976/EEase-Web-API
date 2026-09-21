using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.UpdateUser
{
    public sealed class UsernameAlreadyTakenException : BaseException
    {
        public UsernameAlreadyTakenException()
            : base("Username must be unique.", (int)StatusEnum.UsernameAlreadyTaken) { }
    }
}
