namespace EEaseWebAPI.Application.Options
{
    public sealed class CacheOptions
    {
        public const string SectionName = "CacheConfiguration";

        public string AllCitiesCacheKey { get; init; } = "AllWorldCities_Cache";

        public string AllCurrenciesCacheKey { get; init; } = "AllCurrencies_Cache";

        public int ReferenceDataLifetimeHours { get; init; } = 24;
    }
}
