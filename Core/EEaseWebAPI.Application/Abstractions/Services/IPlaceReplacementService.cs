using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPlaceReplacementService
    {
        Task<TravelDay> ReplaceAsync(
            StandardRoute route,
            PreferenceProfile profile,
            string googlePlaceId,
            string placeType,
            IReadOnlyCollection<string> alsoExclude,
            CancellationToken cancellationToken = default);
    }
}
