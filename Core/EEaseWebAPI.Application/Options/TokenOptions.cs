using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class TokenOptions
    {
        public const string SectionName = "Token";

        [Required(AllowEmptyStrings = false, ErrorMessage = "Token:Audience must be configured.")]
        public string Audience { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Token:Issuer must be configured.")]
        public string Issuer { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false, ErrorMessage = "Token:SecurityKey must be configured.")]
        [MinLength(32, ErrorMessage = "Token:SecurityKey must be at least 32 characters (HMAC-SHA256).")]
        public string SecurityKey { get; init; } = string.Empty;

        [Range(60, 86400)]
        public int AccessTokenLifetimeSeconds { get; init; } = 3600;

        [Range(60, 31536000)]
        public int RefreshTokenLifetimeSeconds { get; init; } = 1296000;
    }
}
