using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Exceptions.UpdateUser
{
    /// <summary>
    /// A field on the update request breaks a rule. The validator normally answers first
    /// with the offending field named; this is what a direct call to the service gets.
    /// </summary>
    public sealed class InvalidUserDataException : BaseException
    {
        public InvalidUserDataException(string message)
            : base(message, (int)StatusEnum.InvalidUserData) { }
    }
}
