using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class CacheOptions
    {
        public const string SectionName = "CacheConfiguration";

        [Required(AllowEmptyStrings = false)]
        public string AllCitiesCacheKey { get; init; } = "AllWorldCities_Cache";

        [Required(AllowEmptyStrings = false)]
        public string CityNamesCacheKey { get; init; } = "CityNames_Cache";

        [Required(AllowEmptyStrings = false)]
        public string AllCountriesCacheKey { get; init; } = "AllCountries_Cache";

        [Required(AllowEmptyStrings = false)]
        public string AllCurrenciesCacheKey { get; init; } = "AllCurrencies_Cache";

        [Required(AllowEmptyStrings = false)]
        public string UsersCacheKey { get; init; } = "AllUsers_Cache";

        [Range(1, 720)]
        public int ReferenceDataLifetimeHours { get; init; } = 24;

        [Range(1, 720)]
        public int UserLifetimeHours { get; init; } = 1;
    }
}
