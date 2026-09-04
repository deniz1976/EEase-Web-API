using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class MailOptions
    {
        public const string SectionName = "MailService";

        [Required(AllowEmptyStrings = false)]
        public string Host { get; init; } = "smtp.gmail.com";

        [Range(1, 65535)]
        public int Port { get; init; } = 587;

        public string Key { get; init; } = string.Empty;

        public string Name { get; init; } = string.Empty;

        public string Email { get; init; } = string.Empty;

        public bool Enabled { get; init; } = true;
    }
}
