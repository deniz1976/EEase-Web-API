namespace EEaseWebAPI.Application.Exceptions
{
    public sealed class MailDeliveryException : BaseException
    {
        public MailDeliveryException(string message, int statusEnum) : base(message, statusEnum) { }
    }
}
