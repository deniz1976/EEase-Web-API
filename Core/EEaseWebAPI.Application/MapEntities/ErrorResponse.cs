using System.Text.Json.Serialization;

namespace EEaseWebAPI.Application.MapEntities
{
    public sealed class ErrorResponse : ApiResponse<ErrorBody>
    {
    }

    public sealed class ErrorBody
    {
        public int StatusCode { get; set; }

        public string? Title { get; set; }

        public string? Message { get; set; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
    }
}
