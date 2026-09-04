using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.GetAllCountries;
using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities.Cities;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Services
{
    public class CityService : ICityService
    {
        private readonly EEaseAPIDbContext _context;
        private readonly IMemoryCache _memoryCache;
        private readonly UserManager<AppUser> _userManager;
        private readonly string _citiesCacheKey;
        private readonly string _countriesCacheKey;
        private readonly string _allWorldCitiesCacheKey;
        private const string DEFAULT_COUNTRY = "Turkey";

        public CityService(
            EEaseAPIDbContext context, 
            IMemoryCache memoryCache, 
            IConfiguration configuration,
            UserManager<AppUser> userManager)
        {
            _context = context;
            _memoryCache = memoryCache;
            _userManager = userManager;
            _citiesCacheKey = configuration["CacheConfiguration:AllCitiesCacheKey"] ?? "AllCities_Cache";
            _countriesCacheKey = configuration["CacheConfiguration:AllCountriesCacheKey"] ?? "AllCountries_Cache";
            _allWorldCitiesCacheKey = configuration["CacheConfiguration:AllWorldCitiesCacheKey"] ?? "AllWorldCities_Cache";
        }

        public async Task<List<string>> GetAllCityNames()
        {
            try
            {
                if (_memoryCache.TryGetValue(_citiesCacheKey, out List<string> cachedCities))
                {
                    return cachedCities;
                }

                var cities = await _context.AllWorldCities
                    .AsNoTracking()
                    .OrderByDescending(c => c.capital == "primary")
                    .ThenByDescending(c => c.population)
                    .Select(c => c.city ?? c.city_ascii ?? "")
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24));

                _memoryCache.Set(_citiesCacheKey, cities, cacheOptions);

                return cities;
            }
            catch (Exception ex)
            {
                throw new GetAllCountriesFailedException($"Failed to get all city names: {ex.Message}", ex);
            }
        }

        public async Task<List<string>> GetAllCountries()
        {
            try
            {
                if (_memoryCache.TryGetValue(_countriesCacheKey, out List<string> cachedCountries))
                {
                    return cachedCountries;
                }

                var countries = await _context.AllWorldCities
                    .AsNoTracking()
                    .Select(c => c.country ?? "")
                    .Where(c => !string.IsNullOrEmpty(c))
                    .Distinct()
                    .OrderBy(c => c)
                    .ToListAsync();

                var cacheOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(24));

                _memoryCache.Set(_countriesCacheKey, countries, cacheOptions);

                return countries;
            }
            catch (Exception ex)
            {
                throw new GetAllCountriesFailedException($"Failed to get all countries: {ex.Message}", ex);
            }
        }

        private async Task<List<AllWorldCities>> GetAllWorldCitiesFromCache()
        {
            if (_memoryCache.TryGetValue(_allWorldCitiesCacheKey, out List<AllWorldCities> cachedCities))
            {
                return cachedCities;
            }

            var cities = await _context.AllWorldCities
                .AsNoTracking()
                .OrderByDescending(c => c.capital == "primary")
                .ThenByDescending(c => c.population)
                .ToListAsync();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(24));

            _memoryCache.Set(_allWorldCitiesCacheKey, cities, cacheOptions);

            return cities;
        }

        public async Task<(List<CityDto> Cities, int TotalCount)> GetCitiesBySearchAsync(string searchTerm, int pageSize, int pageNumber, string? username)
        {
            try
            {
                if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < 2)
                    throw new InvalidSearchTermException();

                var allCities = await GetAllWorldCitiesFromCache();

                string userCountry = DEFAULT_COUNTRY;
                if (!string.IsNullOrEmpty(username))
                {
                    var user = await _userManager.FindByNameAsync(username);
                    if (user != null && !string.IsNullOrEmpty(user.Country))
                    {
                        userCountry = user.Country;
                    }
                }

                var filteredCities = allCities
                    .Where(c => (c.city != null && c.city.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)) || 
                               (c.city_ascii != null && c.city_ascii.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
                    .OrderByDescending(c => c.country == userCountry) 
                    .ThenByDescending(c => c.capital == "primary") 
                    .ThenByDescending(c => c.population) 
                    .ToList();

                var totalCount = filteredCities.Count;

                var pagedCities = filteredCities
                    .Skip((pageNumber - 1) * pageSize)
                    .Take(pageSize)
                    .Select(c => new CityDto
                    {
                        CityName = c.city ?? c.city_ascii ?? "",
                        Country = c.country ?? ""
                    })
                    .ToList();

                return (pagedCities, totalCount);
            }
            catch (InvalidSearchTermException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new GetCitiesBySearchFailedException($"Failed to search cities: {ex.Message}", ex);
            }
        }

        public async Task InitializeCacheAsync()
        {
            try
            {
                if (!_memoryCache.TryGetValue(_allWorldCitiesCacheKey, out _))
                {
                    await GetAllWorldCitiesFromCache();
                }

                if (!_memoryCache.TryGetValue(_citiesCacheKey, out _))
                {
                    await GetAllCityNames();
                }

                if (!_memoryCache.TryGetValue(_countriesCacheKey, out _))
                {
                    await GetAllCountries();
                }
            }
            catch (Exception ex)
            {
                throw new GetAllCountriesFailedException($"Failed to initialize cache: {ex.Message}", ex);
            }
        }
    }
} 