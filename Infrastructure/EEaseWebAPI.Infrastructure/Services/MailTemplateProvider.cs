using System.Collections.Concurrent;
using System.Reflection;
using EEaseWebAPI.Application;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Infrastructure.Services
{
    public sealed class MailTemplateProvider
    {
        private const string ResourcePrefix = "EEaseWebAPI.Infrastructure.Templates.";

        private static readonly Assembly TemplateAssembly = typeof(MailTemplateProvider).Assembly;

        private readonly ConcurrentDictionary<string, string> _cache = new();
        private readonly IStringLocalizer<AppMessages> _messages;

        public MailTemplateProvider(IStringLocalizer<AppMessages> messages)
        {
            _messages = messages;
        }

        /// <summary>
        /// Fills a template with the code and the wording for the recipient's language.
        /// The markup is shared by every language; only the words come from the resources.
        /// </summary>
        public string Render(string templateName, string code)
        {
            var template = _cache.GetOrAdd(templateName, Load);

            return template
                .Replace("{{Code}}", code, StringComparison.Ordinal)
                .Replace("{{CodeLabel}}", _messages[$"Mail_{templateName}Label"], StringComparison.Ordinal)
                .Replace("{{HeadlineLine1}}", _messages["Mail_HeadlineLine1"], StringComparison.Ordinal)
                .Replace("{{HeadlineLine2}}", _messages["Mail_HeadlineLine2"], StringComparison.Ordinal)
                .Replace("{{HeadlineLine3}}", _messages["Mail_HeadlineLine3"], StringComparison.Ordinal)
                .Replace("{{Rights}}", _messages["Mail_Rights"], StringComparison.Ordinal);
        }

        private static string Load(string templateName)
        {
            var resourceName = $"{ResourcePrefix}{templateName}.html";

            using var stream = TemplateAssembly.GetManifestResourceStream(resourceName)
                ?? throw new InvalidOperationException(
                    $"The '{resourceName}' email template was not found among the embedded resources.");

            using var reader = new StreamReader(stream);
            return reader.ReadToEnd();
        }
    }
}
