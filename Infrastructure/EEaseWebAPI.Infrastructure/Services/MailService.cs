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

        public void SendEmail(string email, string subject, string message)
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

            using var client = new SmtpClient();

            client.Connect(_options.Host, _options.Port, SecureSocketOptions.StartTls);
            client.Authenticate(_options.Email, _options.Key);
            client.Send(mimeMessage);
            client.Disconnect(quit: true);
        }

        public bool SendVerificationEmail(string email, string subject, string code) =>
            SendTemplate("VerificationCode", email, subject, code);

        public bool SendResetPasswordEmail(string email, string subject, string code) =>
            SendTemplate("ResetPassword", email, subject, code);

        public bool SendDeleteCodeEmail(string email, string subject, string code) =>
            SendTemplate("DeleteAccount", email, subject, code);

        private bool SendTemplate(string templateName, string email, string subject, string code)
        {
            try
            {
                SendEmail(email, subject, _templateProvider.Render(templateName, code));
                return true;
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Could not send the {Template} email. Recipient: {Email}",
                    templateName,
                    email);

                return false;
            }
        }
    }
}
