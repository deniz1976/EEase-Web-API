namespace EEaseWebAPI.Application.Options
{
    public sealed class CorsOptions
    {
        public const string SectionName = "Cors";

        public IReadOnlyList<string> AllowedOrigins { get; init; } = Array.Empty<string>();

        public bool AllowCredentials { get; init; }
    }
}
