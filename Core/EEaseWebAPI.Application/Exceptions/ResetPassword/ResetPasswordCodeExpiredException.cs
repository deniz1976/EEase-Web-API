namespace EEaseWebAPI.Application.Exceptions.ResetPassword
{
    public class ResetPasswordCodeExpiredException : BaseException
    {
        public ResetPasswordCodeExpiredException(string message, int statusenumcode) : base(message, statusenumcode) { }
    }
}
