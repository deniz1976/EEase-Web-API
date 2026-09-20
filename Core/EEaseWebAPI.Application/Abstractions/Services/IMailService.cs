namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Talking to an SMTP server takes as long as the server takes. Every send is awaited so
    /// that wait never sits on a request thread.
    /// </summary>
    public interface IMailService
    {
        Task SendEmailAsync(
            string email, string subject, string message, CancellationToken cancellationToken = default);

        /// <summary>Returns false when the mail could not be handed to the server.</summary>
        Task<bool> SendVerificationEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default);

        Task<bool> SendResetPasswordEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default);

        Task<bool> SendDeleteCodeEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default);
    }
}
