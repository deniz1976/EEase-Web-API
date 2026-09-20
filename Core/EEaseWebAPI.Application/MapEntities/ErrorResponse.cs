using System.Text.Json.Serialization;

namespace EEaseWebAPI.Application.MapEntities
{
    /// <summary>
    /// A failure is answered in the same envelope as a success, so a caller reads one shape
    /// whatever happened: the header says it failed and carries the status code, the body
    /// says what went wrong.
    /// </summary>
    public sealed class ErrorResponse : ApiResponse<ErrorBody>
    {
    }

    public sealed class ErrorBody
    {
        public int StatusCode { get; set; }

        public string? Title { get; set; }

        public string? Message { get; set; }

        /// <summary>Set only when a request failed validation, keyed by field.</summary>
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IReadOnlyDictionary<string, string[]>? Errors { get; set; }
    }
}
