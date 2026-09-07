using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGooglePlacesService
    {
        Task<PlaceSearchResponse> SearchPlacesAsync(string query, string? type = null);

        Task<string> GetPlaceDetailsAsync(string placeId);

        Task<GetRouteComponentPhotoCommandResponseBody> GetPlacePhotosAsync(string photoName, int maxWidth = 400, int maxHeight = 400);
    }
}
