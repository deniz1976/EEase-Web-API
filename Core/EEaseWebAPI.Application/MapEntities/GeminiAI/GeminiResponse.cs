using System.Text.Json.Serialization;

namespace EEaseWebAPI.Application.MapEntities.GeminiAI
{
    public class GeminiResponse
    {
        [JsonPropertyName("candidates")]
        public Candidate[]? Candidates { get; set; }

        [JsonPropertyName("promptFeedback")]
        public PromptFeedback? PromptFeedback { get; set; }
    }

    public class Candidate
    {
        [JsonPropertyName("content")]
        public Content? Content { get; set; }

        [JsonPropertyName("finishReason")]
        public string? FinishReason { get; set; }

        [JsonPropertyName("index")]
        public int Index { get; set; }

        [JsonPropertyName("safetyRatings")]
        public SafetyRating[]? SafetyRatings { get; set; }
    }

    public class Content
    {
        [JsonPropertyName("parts")]
        public Part[]? Parts { get; set; }

        [JsonPropertyName("role")]
        public string? Role { get; set; }
    }

    public class Part
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class SafetyRating
    {
        [JsonPropertyName("category")]
        public string? Category { get; set; }

        [JsonPropertyName("probability")]
        public string? Probability { get; set; }
    }

    public class PromptFeedback
    {
        [JsonPropertyName("safetyRatings")]
        public SafetyRating[]? SafetyRatings { get; set; }
    }
} 