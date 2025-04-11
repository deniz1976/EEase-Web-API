using EEaseWebAPI.Application.MapEntities.Cities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Manages city-related operations including searching, listing, and caching of city information.
    /// Provides functionality for retrieving and managing city and country data.
    /// </summary>
    public interface ICityService
    {
        /// <summary>
        /// Retrieves a list of all city names in the system.
        /// </summary>
        /// <returns>List of city names</returns>
        /// <remarks>
        /// - Returns cached city names when available
        /// - Includes all supported cities in the system
        /// - Names are returned in a standardized format
        /// </remarks>
        Task<List<string>> GetAllCityNames();

        /// <summary>
        /// Retrieves a list of all countries in the system.
        /// </summary>
        /// <returns>List of country names</returns>
        /// <remarks>
        /// - Returns cached country names when available
        /// - Includes all countries with supported cities
        /// - Names are returned in a standardized format
        /// </remarks>
        Task<List<string>> GetAllCountries();

        /// <summary>
        /// Searches for cities based on a search term with pagination support.
        /// </summary>
        /// <param name="searchTerm">Term to search for in city names</param>
        /// <param name="pageSize">Number of results per page</param>
        /// <param name="pageNumber">Page number to retrieve</param>
        /// <param name="username">Optional username for personalized results</param>
        /// <returns>Tuple containing list of matching cities and total count</returns>
        /// <remarks>
        /// - Supports partial name matching
        /// - Includes pagination for large result sets
        /// - Can personalize results based on user preferences
        /// - Returns both city information and total count for pagination
        /// </remarks>
        Task<(List<CityDto> Cities, int TotalCount)> GetCitiesBySearchAsync(string searchTerm, int pageSize, int pageNumber, string? username);

        /// <summary>
        /// Initializes the cache for city and country data.
        /// </summary>
        /// <returns>Task representing the cache initialization process</returns>
        /// <remarks>
        /// - Loads city and country data into memory
        /// - Optimizes subsequent queries
        /// - Should be called during application startup
        /// </remarks>
        Task InitializeCacheAsync();
    }
} 