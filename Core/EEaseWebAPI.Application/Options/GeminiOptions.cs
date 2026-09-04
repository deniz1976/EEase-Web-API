using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class GeminiOptions
    {
        public const string SectionName = "GeminiAI";

        public IReadOnlyList<string> ApiKeys { get; init; } = Array.Empty<string>();

        [Required(AllowEmptyStrings = false)]
        public string BaseAddress { get; init; } = "https://generativelanguage.googleapis.com/";

        [Required(AllowEmptyStrings = false)]
        public string ApiVersion { get; init; } = "v1beta";

        [Required(AllowEmptyStrings = false)]
        public string Model { get; init; } = "gemini-3.8-flash";

        [Range(1, 10000)]
        public int RequestsPerMinutePerKey { get; init; } = 100;

        [Range(5, 600)]
        public int TimeoutSeconds { get; init; } = 120;

        [Range(0, 10)]
        public int MaxRetryCount { get; init; } = 3;

        [Range(0.0, 2.0)]
        public double Temperature { get; init; } = 1.0;

        [Range(256, 65536)]
        public int MaxOutputTokens { get; init; } = 8192;

        public string GenerateContentPath => $"{ApiVersion}/models/{Model}:generateContent";
    }
}
