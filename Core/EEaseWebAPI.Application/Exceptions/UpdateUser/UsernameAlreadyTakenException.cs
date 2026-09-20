using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.UpdateUser
{
    /// <summary>
    /// Only the database can answer this one, so it stays in the service rather than moving
    /// to the validator with the other username rules.
    /// </summary>
    public sealed class UsernameAlreadyTakenException : BaseException
    {
        public UsernameAlreadyTakenException()
            : base("Username must be unique.", (int)StatusEnum.UsernameAlreadyTaken) { }
    }
}
