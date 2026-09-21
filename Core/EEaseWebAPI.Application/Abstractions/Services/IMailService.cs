namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IMailService
    {
        Task SendEmailAsync(
            string email, string subject, string message, CancellationToken cancellationToken = default);

        Task<bool> SendVerificationEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default);

        Task<bool> SendResetPasswordEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default);

        Task<bool> SendDeleteCodeEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default);
    }
}
