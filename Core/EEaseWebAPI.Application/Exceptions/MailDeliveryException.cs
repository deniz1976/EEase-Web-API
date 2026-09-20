namespace EEaseWebAPI.Application.Exceptions
{
    /// <summary>
    /// The code was stored but the mail carrying it never left. Telling the caller their code
    /// is on its way would leave them waiting for something that is not coming.
    /// </summary>
    public sealed class MailDeliveryException : BaseException
    {
        public MailDeliveryException(string message, int statusEnum) : base(message, statusEnum) { }
    }
}
