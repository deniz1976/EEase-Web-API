using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGooglePlacesService
    {
        Task<PlaceSearchResponse> SearchPlacesAsync(string query, CancellationToken cancellationToken = default);

        Task<string> GetPlaceDetailsAsync(string placeId, CancellationToken cancellationToken = default);

        Task<GetRouteComponentPhotoCommandResponseBody> GetPlacePhotosAsync(
            string photoName,
            int maxWidth = 400,
            int maxHeight = 400,
            CancellationToken cancellationToken = default);
    }
}
