using System.Text.Json.Serialization;

namespace EEaseWebAPI.Application.MapEntities.GeminiAI
{
    public class GeminiResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("object")]
        public string? Object { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("service_tier")]
        public string? ServiceTier { get; set; }

        [JsonPropertyName("created")]
        public DateTimeOffset? Created { get; set; }

        [JsonPropertyName("updated")]
        public DateTimeOffset? Updated { get; set; }

        [JsonPropertyName("usage")]
        public GeminiUsage? Usage { get; set; }

        [JsonPropertyName("steps")]
        public GeminiStep[]? Steps { get; set; }
    }

    public class GeminiStep
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("signature")]
        public string? Signature { get; set; }

        [JsonPropertyName("summary")]
        public string? Summary { get; set; }

        [JsonPropertyName("content")]
        public GeminiStepContent[]? Content { get; set; }
    }

    public class GeminiStepContent
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("text")]
        public string? Text { get; set; }
    }

    public class GeminiUsage
    {
        [JsonPropertyName("total_tokens")]
        public int TotalTokens { get; set; }

        [JsonPropertyName("total_input_tokens")]
        public int TotalInputTokens { get; set; }

        [JsonPropertyName("input_tokens_by_modality")]
        public GeminiModalityTokenCount[]? InputTokensByModality { get; set; }

        [JsonPropertyName("total_cached_tokens")]
        public int TotalCachedTokens { get; set; }

        [JsonPropertyName("total_output_tokens")]
        public int TotalOutputTokens { get; set; }

        [JsonPropertyName("total_tool_use_tokens")]
        public int TotalToolUseTokens { get; set; }

        [JsonPropertyName("total_thought_tokens")]
        public int TotalThoughtTokens { get; set; }

        [JsonPropertyName("raw_prompt_token")]
        public int RawPromptToken { get; set; }

        [JsonPropertyName("model_invocation_token_counts")]
        public GeminiModelInvocationTokenCount[]? ModelInvocationTokenCounts { get; set; }
    }

    public class GeminiModelInvocationTokenCount
    {
        [JsonPropertyName("prompt_tokens_details")]
        public GeminiModalityTokenCount[]? PromptTokensDetails { get; set; }

        [JsonPropertyName("candidates_tokens_details")]
        public GeminiModalityTokenCount[]? CandidatesTokensDetails { get; set; }

        [JsonPropertyName("thoughts_tokens_details")]
        public GeminiModalityTokenCount[]? ThoughtsTokensDetails { get; set; }
    }

    public class GeminiModalityTokenCount
    {
        [JsonPropertyName("modality")]
        public string? Modality { get; set; }

        [JsonPropertyName("tokens")]
        public int Tokens { get; set; }
    }
}
