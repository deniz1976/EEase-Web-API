using System.Collections.Concurrent;
using System.Reflection;
using EEaseWebAPI.Application.Resources;

namespace EEaseWebAPI.Infrastructure.Services
{
    public enum MailTemplate
    {
        VerificationCode,
        ResetPassword,
        DeleteAccount
    }

    public sealed class MailTemplateProvider
    {
        private const string ResourcePrefix = "EEaseWebAPI.Infrastructure.Templates.";

        private static readonly Assembly TemplateAssembly = typeof(MailTemplateProvider).Assembly;

        private readonly ConcurrentDictionary<MailTemplate, string> _cache = new();

        /// <summary>
        /// Fills a template with the code and the wording for the recipient's language.
        /// The markup is shared by every language; only the words come from the resources.
        /// </summary>
        public string Render(MailTemplate template, string code)
        {
            var markup = _cache.GetOrAdd(template, Load);

            return markup
                .Replace("{{Code}}", code, StringComparison.Ordinal)
                .Replace("{{CodeLabel}}", CodeLabel(template), StringComparison.Ordinal)
                .Replace("{{HeadlineLine1}}", AppMessages.Mail_HeadlineLine1, StringComparison.Ordinal)
                .Replace("{{HeadlineLine2}}", AppMessages.Mail_HeadlineLine2, StringComparison.Ordinal)
                .Replace("{{HeadlineLine3}}", AppMessages.Mail_HeadlineLine3, StringComparison.Ordinal)
                .Replace("{{Rights}}", AppMessages.Mail_Rights, StringComparison.Ordinal);
        }

        private static string CodeLabel(MailTemplate template) => template switch
        {
            MailTemplate.VerificationCode => AppMessages.Mail_VerificationCodeLabel,
            MailTemplate.ResetPassword => AppMessages.Mail_ResetPasswordLabel,
            MailTemplate.DeleteAccount => AppMessages.Mail_DeleteAccountLabel,
            _ => throw new InvalidOperationException($"No code label is defined for '{template}'.")
        };

        private static string Load(MailTemplate template)
        {
            var resourceName = $"{ResourcePrefix}{template}.html";

            using var stream = TemplateAssembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"The '{resourceName}' email template was not found among the embedded resources.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
