using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class GooglePlacesOptions
    {
        public const string SectionName = "GooglePlaces";

        public string ApiKey { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string BaseAddress { get; init; } = "https://places.googleapis.com/";

        [Range(5, 300)]
        public int TimeoutSeconds { get; init; } = 60;
    }
}
