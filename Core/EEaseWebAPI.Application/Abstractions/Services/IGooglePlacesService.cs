using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Features.Queries.Place.GetPlacePhoto;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGooglePlacesService
    {
        Task<PlaceSearchResponse> SearchPlacesAsync(string query, CancellationToken cancellationToken = default);

        Task<string> GetPlaceDetailsAsync(string placeId, CancellationToken cancellationToken = default);

        Task<GetPlacePhotoQueryResponseBody> GetPlacePhotosAsync(
            string photoName,
            int maxWidth = 400,
            int maxHeight = 400,
            CancellationToken cancellationToken = default);
    }
}
