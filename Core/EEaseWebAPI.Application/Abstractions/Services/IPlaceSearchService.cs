using EEaseWebAPI.Application.DTOs.GooglePlaces;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPlaceSearchService
    {
        Task<IReadOnlyList<Place>> SearchAsync(string query, CancellationToken cancellationToken = default);

        Task<IReadOnlyList<Place>> SearchFirstMatchAsync(
            IEnumerable<string> queries,
            CancellationToken cancellationToken = default);

        Task<IReadOnlyList<string>> CollectPlaceIdsAsync(
            IEnumerable<string> queries,
            int requiredCount = 0,
            IEnumerable<string>? excludedPlaceIds = null,
            CancellationToken cancellationToken = default);
    }
}
