using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Application.DTOs.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;
using EEaseWebAPI.Application.DTOs.Route.DislikePlaceOrRestaurantDTO;
using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRouteService
    {
        Task<CreateRouteWithoutLoginCommandResponseBody> CreateRouteWithoutLogin(string? destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL);

        Task<(List<StandardRoute> Routes, int TotalCount)> GetAllRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetRoutesByUserId(string userId, string requesterUsername, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);


        /// <summary>
        /// Likes or unlikes a route for a specific user
        /// </summary>
        /// <returns>True if the route is now liked, false if unliked</returns>
        Task<bool> LikeRoute(string username, Guid routeId);

        /// <summary>
        /// Deletes a route by its ID if the user is authorized
        /// </summary>
        /// <param name="username">The username of the user attempting to delete the route</param>
        /// <param name="routeId">The ID of the route to delete</param>
        /// <returns>True if the route was successfully deleted, false otherwise</returns>
        Task<bool> DeleteRoute(string username, Guid? routeId);

        Task<CreateCustomRouteCommandResponseBody> CreateCustomRoute(string? destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL, string? username, List<string>? usernames);

        /// <summary>
        /// Retrieves a specific route by its ID with access control based on route status.
        /// This method checks the route's visibility status and user's permissions:
        /// - Status 0: Private, only visible to route owner
        /// - Status 1: Friends only, visible to route owner and their friends
        /// - Status 2: Public, visible to everyone
        /// </summary>
        /// <param name="username">The username of the user requesting the route</param>
        /// <param name="routeId">The unique identifier of the route to retrieve</param>
        /// <returns>The requested route if the user has permission to view it</returns>
        /// <exception cref="UserNotFoundException">Thrown when the requesting user is not found</exception>
        /// <exception cref="RouteNotFoundException">Thrown when the requested route is not found</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user doesn't have permission to view the route</exception>
        Task<StandardRouteDTO> GetRouteById(string username, Guid? routeId);

        Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username);

        /// <summary>
        /// Updates user preferences based on liked places or restaurants.
        /// When a user likes a place or restaurant, their personalization preferences are updated accordingly.
        /// For places: Updates UserPersonalization preferences based on the place type and characteristics
        /// For restaurants: Updates UserFoodPreferences based on the restaurant type and cuisine
        /// </summary>
        /// <param name="username">The username of the user liking the place/restaurant</param>
        /// <param name="googlePlaceId">The Google Place ID of the place/restaurant</param>
        /// <param name="placeType">The type of place (TravelAccommodation, Breakfast, Lunch, Dinner, PlaceAfterDinner, Place)</param>
        /// <returns>Response indicating whether preferences were updated successfully</returns>
        Task<LikePlaceOrRestaurantCommandResponseBody> LikePlaceOrRestaurantAsync(
            string username,
            string googlePlaceId,
            string placeType);

        /// <summary>
        /// Checks if a user has liked a specific route and has permission to view it.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="routeId">ID of the route to check</param>
        /// <returns>True if the user has liked the route, false otherwise</returns>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user doesn't have permission to view the route</exception>
        /// <exception cref="UserNotFoundException">Thrown when the user is not found</exception>
        /// <exception cref="RouteNotFoundException">Thrown when the route is not found</exception>
        Task<bool> CheckRouteLikeStatus(string username, Guid routeId);

        Task<string> DeleteAllRoutes(string username);

        Task<StandardRoute> DislikePlaceOrRestaurant(DislikePlaceOrRestaurantCommandRequest dislikePlaceOrRestaurantDTO);
    }
}
