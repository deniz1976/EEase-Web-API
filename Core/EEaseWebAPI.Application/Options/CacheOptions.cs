namespace EEaseWebAPI.Application.Options
{
    public sealed class CacheOptions
    {
        public const string SectionName = "CacheConfiguration";

        /// <summary>Every world city row, used to answer city searches.</summary>
        public string AllCitiesCacheKey { get; init; } = "AllWorldCities_Cache";

        /// <summary>Just the city names. A different shape, so a different key.</summary>
        public string CityNamesCacheKey { get; init; } = "CityNames_Cache";

        public string AllCountriesCacheKey { get; init; } = "AllCountries_Cache";

        public string AllCurrenciesCacheKey { get; init; } = "AllCurrencies_Cache";

        /// <summary>The searchable list of confirmed users.</summary>
        public string UsersCacheKey { get; init; } = "AllUsers_Cache";

        public int ReferenceDataLifetimeHours { get; init; } = 24;

        /// <summary>Users change far more often than cities do.</summary>
        public int UserLifetimeHours { get; init; } = 1;
    }
}
