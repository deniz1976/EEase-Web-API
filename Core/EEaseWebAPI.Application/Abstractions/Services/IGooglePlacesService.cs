using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Provides integration with Google Places API for location-based services.
    /// Handles place searches, details retrieval, and photo management.
    /// </summary>
    public interface IGooglePlacesService
    {
        /// <summary>
        /// Searches for places using the Google Places API based on a query string.
        /// </summary>
        /// <param name="query">Search query for finding places</param>
        /// <param name="type">Optional type of place to filter results (e.g., restaurant, hotel)</param>
        /// <returns>Response containing matching places and their basic information</returns>
        /// <remarks>
        /// - Supports text-based place searches
        /// - Can filter by place type
        /// - Returns relevant place details including IDs and basic info
        /// </remarks>
        Task<PlaceSearchResponse> SearchPlacesAsync(string query, string? type = null);

        /// <summary>
        /// Retrieves detailed information about a specific place using its Place ID.
        /// </summary>
        /// <param name="placeId">Google Places API Place ID</param>
        /// <returns>JSON string containing comprehensive place details</returns>
        /// <remarks>
        /// Includes information such as:
        /// - Contact details
        /// - Opening hours
        /// - Reviews and ratings
        /// - Address information
        /// - Available facilities
        /// </remarks>
        Task<string> GetPlaceDetailsAsync(string placeId);

        /// <summary>
        /// Retrieves photos for a place using the photo reference.
        /// </summary>
        /// <param name="photoName">Photo reference string from Google Places API</param>
        /// <param name="maxWidth">Maximum width of the photo (default: 400)</param>
        /// <param name="maxHeight">Maximum height of the photo (default: 400)</param>
        /// <returns>Response containing the photo data and metadata</returns>
        /// <remarks>
        /// - Handles photo sizing and optimization
        /// - Manages API attribution requirements
        /// - Supports various photo formats
        /// </remarks>
        Task<GetRouteComponentPhotoCommandResponseBody> GetPlacePhotosAsync(string photoName, int maxWidth = 400, int maxHeight = 400);
    }
} 