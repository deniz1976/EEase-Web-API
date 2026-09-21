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

        public Task<List<string>> GetAllCityNames(CancellationToken cancellationToken = default) =>
            _cache.GetOrLoadAsync(
                _cache.Keys.CityNamesCacheKey,
                async token => CitySearch.NamesOf(await GetAllCitiesAsync(token)),
                cancellationToken);

        public Task<List<string>> GetAllCountries(CancellationToken cancellationToken = default) =>
            _cache.GetOrLoadAsync(
                _cache.Keys.AllCountriesCacheKey,
                async token => CitySearch.CountriesOf(await GetAllCitiesAsync(token)),
                cancellationToken);

        public async Task<(List<CityDto> Cities, int TotalCount)> SearchCitiesAsync(
            string searchTerm,
            int pageSize,
            int pageNumber,
            string? username,
            CancellationToken cancellationToken = default)
        {
            var cities = await GetAllCitiesAsync(cancellationToken);
            var homeCountry = await HomeCountryOfAsync(username);

            return CitySearch.Search(cities, searchTerm, homeCountry, pageSize, pageNumber);
        }

        public async Task InitializeCacheAsync(CancellationToken cancellationToken = default)
        {
            await GetAllCitiesAsync(cancellationToken);
            await GetAllCityNames(cancellationToken);
            await GetAllCountries(cancellationToken);
        }

        private Task<List<AllWorldCities>> GetAllCitiesAsync(CancellationToken cancellationToken) =>
            _cache.GetOrLoadAsync(
                _cache.Keys.AllCitiesCacheKey,
                async token => CitySearch.Rank(
                    await _context.AllWorldCities.AsNoTracking().ToListAsync(token)),
                cancellationToken);

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
