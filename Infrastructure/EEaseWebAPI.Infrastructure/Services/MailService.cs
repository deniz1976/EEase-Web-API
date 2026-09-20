using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace EEaseWebAPI.Infrastructure.Services
{
    public sealed class MailService : IMailService
    {
        private readonly MailOptions _options;
        private readonly MailTemplateProvider _templateProvider;
        private readonly ILogger<MailService> _logger;

        public MailService(
            IOptions<MailOptions> options,
            MailTemplateProvider templateProvider,
            ILogger<MailService> logger)
        {
            _options = options.Value;
            _templateProvider = templateProvider;
            _logger = logger;
        }

        public async Task SendEmailAsync(
            string email, string subject, string message, CancellationToken cancellationToken = default)
        {
            if (!_options.Enabled)
            {
                _logger.LogInformation(
                    "Mail delivery is disabled (MailService:Enabled = false). Recipient: {Email}, Subject: {Subject}",
                    email,
                    subject);
                return;
            }

            if (string.IsNullOrWhiteSpace(_options.Email) || string.IsNullOrWhiteSpace(_options.Key))
            {
                throw new InvalidOperationException(
                    "SMTP credentials are missing. Set 'MailService:Email' and 'MailService:Key', " +
                    "or set 'MailService:Enabled' to false.");
            }

            var mimeMessage = new MimeMessage
            {
                Subject = subject,
                Body = new BodyBuilder { HtmlBody = message }.ToMessageBody()
            };

            mimeMessage.From.Add(new MailboxAddress(_options.Name, _options.Email));
            mimeMessage.To.Add(new MailboxAddress(email, email));

            using var client = new SmtpClient
            {
                Timeout = (int)TimeSpan.FromSeconds(_options.TimeoutSeconds).TotalMilliseconds
            };

            await client.ConnectAsync(_options.Host, _options.Port, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(_options.Email, _options.Key, cancellationToken);
            await client.SendAsync(mimeMessage, cancellationToken);
            await client.DisconnectAsync(quit: true, cancellationToken);
        }

        public Task<bool> SendVerificationEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default) =>
            SendTemplateAsync(MailTemplate.VerificationCode, email, subject, code, cancellationToken);

        public Task<bool> SendResetPasswordEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default) =>
            SendTemplateAsync(MailTemplate.ResetPassword, email, subject, code, cancellationToken);

        public Task<bool> SendDeleteCodeEmailAsync(
            string email, string subject, string code, CancellationToken cancellationToken = default) =>
            SendTemplateAsync(MailTemplate.DeleteAccount, email, subject, code, cancellationToken);

        private async Task<bool> SendTemplateAsync(
            MailTemplate template,
            string email,
            string subject,
            string code,
            CancellationToken cancellationToken)
        {
            try
            {
                await SendEmailAsync(
                    email, subject, _templateProvider.Render(template, code), cancellationToken);

                return true;
            }
            catch (OperationCanceledException)
            {
                // The caller went away; that is not a delivery failure to report.
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Could not send the {Template} email. Recipient: {Email}",
                    template,
                    email);

                return false;
            }
        }
    }
}
