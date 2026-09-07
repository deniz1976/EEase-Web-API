using EEaseWebAPI.Application.MapEntities.Cities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface ICityService
    {
        Task<List<string>> GetAllCityNames();

        Task<List<string>> GetAllCountries();

        Task<(List<CityDto> Cities, int TotalCount)> GetCitiesBySearchAsync(string searchTerm, int pageSize, int pageNumber, string? username);

        Task InitializeCacheAsync();
    }
}
