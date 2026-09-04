using System.Collections.Concurrent;
using System.Reflection;

namespace EEaseWebAPI.Infrastructure.Services
{
    public sealed class MailTemplateProvider
    {
        private const string ResourcePrefix = "EEaseWebAPI.Infrastructure.Templates.";

        private static readonly Assembly TemplateAssembly = typeof(MailTemplateProvider).Assembly;

        private readonly ConcurrentDictionary<string, string> _cache = new();

        public string Render(string templateName, string code)
        {
            var template = _cache.GetOrAdd(templateName, Load);
            return template.Replace("{{Code}}", code, StringComparison.Ordinal);
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
