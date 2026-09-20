using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities.Cities;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using EEaseWebAPI.Persistence.Services.Caching;

namespace EEaseWebAPI.Persistence.Services.ReferenceData
{
    public sealed class CityService : ICityService
    {
        private const string DefaultCountry = "Turkey";
        private const string CapitalCity = "primary";
        private const int MinimumSearchTermLength = 2;

        private readonly IAllWorldCitiesRepository _cities;
        private readonly ReferenceDataCache _cache;
        private readonly UserManager<AppUser> _userManager;

        public CityService(
            IAllWorldCitiesRepository cities,
            ReferenceDataCache cache,
            UserManager<AppUser> userManager)
        {
            _cities = cities;
            _cache = cache;
            _userManager = userManager;
        }

        public Task<List<string>> GetAllCityNames() =>
            _cache.GetOrLoadAsync(_cache.Keys.CityNamesCacheKey, async () =>
                (await GetAllCitiesAsync())
                    .Select(city => city.city ?? city.city_ascii ?? string.Empty)
                    .Where(name => !string.IsNullOrEmpty(name))
                    .Distinct()
                    .ToList());

        public Task<List<string>> GetAllCountries() =>
            _cache.GetOrLoadAsync(_cache.Keys.AllCountriesCacheKey, async () =>
                (await GetAllCitiesAsync())
                    .Select(city => city.country ?? string.Empty)
                    .Where(country => !string.IsNullOrEmpty(country))
                    .Distinct()
                    .OrderBy(country => country)
                    .ToList());

        public async Task<(List<CityDto> Cities, int TotalCount)> GetCitiesBySearchAsync(
            string searchTerm, int pageSize, int pageNumber, string? username)
        {
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < MinimumSearchTermLength)
                throw new InvalidSearchTermException();

            var cities = await GetAllCitiesAsync();
            var homeCountry = await HomeCountryOfAsync(username);

            // The traveller's own country first, then capitals, then the biggest cities:
            // a search for "san" should not open with a village.
            var matches = cities
                .Where(city => Matches(city, searchTerm))
                .OrderByDescending(city => city.country == homeCountry)
                .ThenByDescending(city => city.capital == CapitalCity)
                .ThenByDescending(city => city.population)
                .ToList();

            var page = matches
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(city => new CityDto
                {
                    CityName = city.city ?? city.city_ascii ?? string.Empty,
                    Country = city.country ?? string.Empty
                })
                .ToList();

            return (page, matches.Count);
        }

        public async Task InitializeCacheAsync()
        {
            await GetAllCitiesAsync();
            await GetAllCityNames();
            await GetAllCountries();
        }

        /// <summary>
        /// The whole table, ordered once so that every search starts from the biggest and
        /// most important places, and then kept in memory.
        /// </summary>
        private Task<List<AllWorldCities>> GetAllCitiesAsync() =>
            _cache.GetOrLoadAsync(_cache.Keys.AllCitiesCacheKey, async () =>
                (await _cities.GetAllCitiesAsync())
                    .OrderByDescending(city => city.capital == CapitalCity)
                    .ThenByDescending(city => city.population)
                    .ToList());

        private async Task<string> HomeCountryOfAsync(string? username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return DefaultCountry;
            }

            var user = await _userManager.FindByNameAsync(username);

            return string.IsNullOrEmpty(user?.Country) ? DefaultCountry : user.Country;
        }

        private static bool Matches(AllWorldCities city, string searchTerm) =>
            city.city?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
            city.city_ascii?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true;
    }
}
