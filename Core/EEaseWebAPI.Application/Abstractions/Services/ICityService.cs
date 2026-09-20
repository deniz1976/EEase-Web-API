using EEaseWebAPI.Application.MapEntities.Cities;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface ICityService
    {
        Task<List<string>> GetAllCityNames(CancellationToken cancellationToken = default);

        Task<List<string>> GetAllCountries(CancellationToken cancellationToken = default);

        Task<(List<CityDto> Cities, int TotalCount)> GetCitiesBySearchAsync(
            string searchTerm,
            int pageSize,
            int pageNumber,
            string? username,
            CancellationToken cancellationToken = default);

        Task InitializeCacheAsync(CancellationToken cancellationToken = default);
    }
}
