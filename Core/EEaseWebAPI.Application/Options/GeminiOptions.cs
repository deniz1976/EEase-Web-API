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

        [Required(AllowEmptyStrings = false)]
        public string ApiRevision { get; init; } = "2026-05-20";

        [Range(1, 10000)]
        public int RequestsPerMinutePerKey { get; init; } = 100;

        [Range(5, 600)]
        public int TimeoutSeconds { get; init; } = 120;

        [Range(1, 600)]
        public int QuotaCooldownSeconds { get; init; } = 60;

        [Range(0, 10)]
        public int MaxRetryCount { get; init; } = 3;

        /// <summary>
        /// How long a request waits for a key to free up before giving up. Without a limit
        /// an exhausted pool leaves the request hanging instead of answering.
        /// </summary>
        [Range(1, 300)]
        public int KeyWaitTimeoutSeconds { get; init; } = 30;

        [Range(0.0, 2.0)]
        public double Temperature { get; init; } = 1.0;

        [Range(256, 65536)]
        public int MaxOutputTokens { get; init; } = 8192;

        public string InteractionsPath => $"{ApiVersion}/interactions";
    }
}
