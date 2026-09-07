using System.Text.Json.Serialization;

namespace EEaseWebAPI.Application.DTOs.Route.Enrichment
{
    public sealed class RouteEnrichmentResponse
    {
        [JsonPropertyName("approxPrices")]
        public List<string?>? ApproxPrices { get; set; }

        [JsonPropertyName("star")]
        public string? Star { get; set; }

        [JsonPropertyName("dayDescriptions")]
        public List<string?>? DayDescriptions { get; set; }

        [JsonPropertyName("weathers")]
        public List<List<WeatherForecastDto>?>? Weathers { get; set; }
    }

    public sealed class WeatherForecastDto
    {
        [JsonPropertyName("Degree")]
        public int? Degree { get; set; }

        [JsonPropertyName("Description")]
        public string? Description { get; set; }

        [JsonPropertyName("Warning")]
        public string? Warning { get; set; }

        [JsonPropertyName("Date")]
        public DateOnly? Date { get; set; }
    }
}
