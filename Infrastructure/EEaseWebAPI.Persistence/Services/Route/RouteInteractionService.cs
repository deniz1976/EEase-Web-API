using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Domain.Enums;
using UserNotFoundException = EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException;
using Microsoft.Extensions.Localization;
using EEaseWebAPI.Application;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class RouteInteractionService : IRouteInteractionService
    {
        private const int UserNotFoundCode = 7;
        private const int RouteNotFoundCode = 93;

        private readonly UserManager<AppUser> _userManager;
        private readonly EEaseAPIDbContext _context;
        private readonly IRouteAccessPolicy _routeAccessPolicy;
        private readonly IPreferenceFeedbackService _preferenceFeedbackService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public RouteInteractionService(
            UserManager<AppUser> userManager,
            EEaseAPIDbContext context,
            IRouteAccessPolicy routeAccessPolicy,
            IPreferenceFeedbackService preferenceFeedbackService,

            IStringLocalizer<AppMessages> messages)

        {
            _userManager = userManager;
            _context = context;
            _routeAccessPolicy = routeAccessPolicy;
            _preferenceFeedbackService = preferenceFeedbackService;

            _messages = messages;
        }

        public async Task<bool> LikeRoute(string username, Guid routeId)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .Include(candidate => candidate.LikedUsers)
                .Include(candidate => candidate.User)
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId)
                ?? throw new RouteNotFoundException("Route not found", RouteNotFoundCode);

            var access = await _routeAccessPolicy.EvaluateAsync(route, username, user.Id);

            if (!access.IsAccessible)
                throw new ForbiddenException(access.Message!, StatusEnum.UnauthorizedToViewRoute);

            var wasLiked = route.LikedUsers.Any(liker => liker.Id == user.Id);

            if (wasLiked)
            {
                route.LikedUsers.Remove(user);
            }
            else
            {
                route.LikedUsers.Add(user);
            }

            // Deriving the counter from the collection keeps it from drifting, which a
            // blind increment could not recover from.
            route.LikeCount = route.LikedUsers.Count;

            await _context.SaveChangesAsync();

            return !wasLiked;
        }

        public async Task<bool> DeleteRoute(string username, Guid? routeId)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .Include(candidate => candidate.TravelDays)
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId)
                ?? throw new RouteNotFoundException("Route not found", RouteNotFoundCode);

            if (route.UserId != user.Id)
                throw new DeleteRouteException("Unauthorized to delete route");

            _context.StandardRoutes.Remove(route);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> DeleteAllRoutes(string username)
        {
            var user = await RequireUserAsync(username);

            var routes = await _context.StandardRoutes
                .Where(route => route.UserId == user.Id)
                .ToListAsync();

            if (routes.Count == 0)
                return _messages["NoRoutesToDelete"];

            _context.StandardRoutes.RemoveRange(routes);
            await _context.SaveChangesAsync();

            return _messages["RoutesDeleted"];
        }

        public async Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId)
                ?? throw new RouteNotFoundException("Route not found", RouteNotFoundCode);

            if (route.UserId != user.Id)
                throw new ForbiddenException(
                    "Only route owner can update route status", StatusEnum.UnauthorizedToModifyRoute);

            if (!Enum.IsDefined(typeof(RouteVisibility), status))
                throw new InvalidRouteStatusException();

            route.Status = status;
            await _context.SaveChangesAsync();

            return new UpdateRouteStatusCommandResponseBody { IsUpdated = true };
        }

        public async Task<LikePlaceOrRestaurantCommandResponseBody> LikePlaceOrRestaurantAsync(
            string username, string googlePlaceId, string placeType)
        {
            var user = await RequireUserAsync(username);

            var place = await FindPlaceAsync(googlePlaceId, placeType)
                ?? throw new InvalidPlaceTypeException($"Place not found with Google ID: {googlePlaceId}");

            var feedback = await _preferenceFeedbackService.ApplyAsync(user.Id, place, placeType, liked: true);

            return new LikePlaceOrRestaurantCommandResponseBody
            {
                IsPreferenceUpdated = feedback.HasChanges,
                Message = _messages["PreferencesUpdatedFromFeedback"]
            };
        }

        /// <summary>
        /// A place lives in a different table depending on the slot it fills, so the slot
        /// name decides where to look it up.
        /// </summary>
        private async Task<BaseEntity?> FindPlaceAsync(string googlePlaceId, string placeType)
        {
            // Invariant lowering on purpose: a Turkish locale would turn "I" into a dotless
            // letter and stop the slot names below from matching.
            switch (placeType.ToLowerInvariant())
            {
                case "accommodation":
                    return await _context.TravelAccomodations
                        .Include(place => place.DisplayName)
                        .FirstOrDefaultAsync(place => place.GoogleId == googlePlaceId);

                case "breakfast":
                case "lunch":
                case "dinner":
                case "placeafterdinner":
                    var restaurants = _context.Set<BaseRestaurantPlaceEntity>()
                        .Include(place => place.DisplayName)
                        .Where(place => place.GoogleId == googlePlaceId);

                    return placeType.ToLowerInvariant() switch
                    {
                        "breakfast" => await restaurants.OfType<Breakfast>().FirstOrDefaultAsync(),
                        "lunch" => await restaurants.OfType<Lunch>().FirstOrDefaultAsync(),
                        "dinner" => await restaurants.OfType<Dinner>().FirstOrDefaultAsync(),
                        _ => await restaurants.OfType<PlaceAfterDinner>().FirstOrDefaultAsync()
                    };

                case "firstplace":
                case "secondplace":
                case "thirdplace":
                case "place":
                    return await _context.Places
                        .Include(place => place.DisplayName)
                        .FirstOrDefaultAsync(place => place.GoogleId == googlePlaceId);

                default:
                    throw new InvalidPlaceTypeException($"Invalid place type: {placeType}");
            }
        }

        private async Task<AppUser> RequireUserAsync(string username) =>
            await _userManager.FindByNameAsync(username)
            ?? throw new UserNotFoundException("User not found", UserNotFoundCode);
    }
}
