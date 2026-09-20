using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.MapEntities.Cities;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Caching;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Services.ReferenceData
{
    public sealed class CityService : ICityService
    {
        private const string DefaultCountry = "Turkey";

        private readonly EEaseAPIDbContext _context;
        private readonly ReferenceDataCache _cache;
        private readonly UserManager<AppUser> _userManager;

        public CityService(
            EEaseAPIDbContext context,
            ReferenceDataCache cache,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _cache = cache;
            _userManager = userManager;
        }

        public Task<List<string>> GetAllCityNames() =>
            _cache.GetOrLoadAsync(_cache.Keys.CityNamesCacheKey, async () =>
                CitySearch.NamesOf(await GetAllCitiesAsync()));

        public Task<List<string>> GetAllCountries() =>
            _cache.GetOrLoadAsync(_cache.Keys.AllCountriesCacheKey, async () =>
                CitySearch.CountriesOf(await GetAllCitiesAsync()));

        public async Task<(List<CityDto> Cities, int TotalCount)> GetCitiesBySearchAsync(
            string searchTerm, int pageSize, int pageNumber, string? username)
        {
            var cities = await GetAllCitiesAsync();
            var homeCountry = await HomeCountryOfAsync(username);

            return CitySearch.Search(cities, searchTerm, homeCountry, pageSize, pageNumber);
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
                CitySearch.Rank(await _context.AllWorldCities.AsNoTracking().ToListAsync()));

        private async Task<string> HomeCountryOfAsync(string? username)
        {
            if (string.IsNullOrEmpty(username))
            {
                return DefaultCountry;
            }

            var user = await _userManager.FindByNameAsync(username);

            return string.IsNullOrEmpty(user?.Country) ? DefaultCountry : user.Country;
        }
    }
}
