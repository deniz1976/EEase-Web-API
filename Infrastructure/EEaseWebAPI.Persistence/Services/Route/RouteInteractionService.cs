using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.Features.Commands.Place.LikePlace;
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
using EEaseWebAPI.Application;
using EEaseWebAPI.Application.Resources;

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

        public RouteInteractionService(
            UserManager<AppUser> userManager,
            EEaseAPIDbContext context,
            IRouteAccessPolicy routeAccessPolicy,
            IPreferenceFeedbackService preferenceFeedbackService)
        {
            _userManager = userManager;
            _context = context;
            _routeAccessPolicy = routeAccessPolicy;
            _preferenceFeedbackService = preferenceFeedbackService;
        }

        public Task<int> LikeRouteAsync(string username, Guid routeId, CancellationToken cancellationToken = default) =>
            SetLikeAsync(username, routeId, liked: true, cancellationToken);

        public Task<int> UnlikeRouteAsync(string username, Guid routeId, CancellationToken cancellationToken = default) =>
            SetLikeAsync(username, routeId, liked: false, cancellationToken);

        private async Task<int> SetLikeAsync(
            string username, Guid routeId, bool liked, CancellationToken cancellationToken)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .Include(candidate => candidate.LikedUsers)
                .Include(candidate => candidate.User)
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId, cancellationToken)
                ?? throw new RouteNotFoundException("Route not found", RouteNotFoundCode);

            var access = await _routeAccessPolicy.EvaluateAsync(route, username, user.Id, cancellationToken);

            if (!access.IsAccessible)
                throw new ForbiddenException(access.Message!, StatusEnum.UnauthorizedToViewRoute);

            route.LikedUsers ??= new List<AppUser>();

            var alreadyLiked = route.LikedUsers.Any(liker => liker.Id == user.Id);

            // Asking for what is already the case is not an error: the same request sent
            // twice leaves the route the way the caller asked for it once.
            if (liked && !alreadyLiked)
            {
                route.LikedUsers.Add(user);
            }
            else if (!liked && alreadyLiked)
            {
                route.LikedUsers.Remove(user);
            }

            // Deriving the counter from the collection keeps it from drifting, which a
            // blind increment could not recover from.
            route.LikeCount = route.LikedUsers.Count;

            await _context.SaveChangesAsync(cancellationToken);

            return route.LikeCount ?? 0;
        }

        public async Task<bool> DeleteRoute(string username, Guid? routeId, CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .Include(candidate => candidate.TravelDays)
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId, cancellationToken)
                ?? throw new RouteNotFoundException("Route not found", RouteNotFoundCode);

            if (route.UserId != user.Id)
                throw new DeleteRouteException("Unauthorized to delete route");

            _context.StandardRoutes.Remove(route);
            await _context.SaveChangesAsync(cancellationToken);

            return true;
        }

        public async Task<string> DeleteAllRoutes(string username, CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var routes = await _context.StandardRoutes
                .Where(route => route.UserId == user.Id)
                .ToListAsync(cancellationToken);

            if (routes.Count == 0)
                return AppMessages.NoRoutesToDelete;

            _context.StandardRoutes.RemoveRange(routes);
            await _context.SaveChangesAsync(cancellationToken);

            return AppMessages.RoutesDeleted;
        }

        public async Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username, CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var route = await _context.StandardRoutes
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId, cancellationToken)
                ?? throw new RouteNotFoundException("Route not found", RouteNotFoundCode);

            if (route.UserId != user.Id)
                throw new ForbiddenException(
                    "Only route owner can update route status", StatusEnum.UnauthorizedToModifyRoute);

            if (!Enum.IsDefined(typeof(RouteVisibility), status))
                throw new InvalidRouteStatusException();

            route.Status = status;
            await _context.SaveChangesAsync(cancellationToken);

            return new UpdateRouteStatusCommandResponseBody { IsUpdated = true };
        }

        public async Task<LikePlaceCommandResponseBody> LikePlaceAsync(
            string username,
            string googlePlaceId,
            string placeType,
            CancellationToken cancellationToken = default)
        {
            var user = await RequireUserAsync(username);

            var place = await FindPlaceAsync(googlePlaceId, placeType, cancellationToken)
                ?? throw new InvalidPlaceTypeException($"Place not found with Google ID: {googlePlaceId}");

            var feedback = await _preferenceFeedbackService.ApplyAsync(
                user.Id, place, placeType, liked: true, cancellationToken);

            return new LikePlaceCommandResponseBody
            {
                IsPreferenceUpdated = feedback.HasChanges,
                Message = AppMessages.PreferencesUpdatedFromFeedback
            };
        }

        private async Task<BaseEntity?> FindPlaceAsync(
            string googlePlaceId, string placeType, CancellationToken cancellationToken)
        {
            // Invariant lowering on purpose: a Turkish locale would turn "I" into a dotless
            // letter and stop the slot names below from matching.
            switch (placeType.ToLowerInvariant())
            {
                case "accommodation":
                    return await _context.TravelAccomodations
                        .Include(place => place.DisplayName)
                        .FirstOrDefaultAsync(place => place.GoogleId == googlePlaceId, cancellationToken);

                case "breakfast":
                case "lunch":
                case "dinner":
                case "placeafterdinner":
                    var restaurants = _context.Set<BaseRestaurantPlaceEntity>()
                        .Include(place => place.DisplayName)
                        .Where(place => place.GoogleId == googlePlaceId);

                    return placeType.ToLowerInvariant() switch
                    {
                        "breakfast" => await restaurants.OfType<Breakfast>().FirstOrDefaultAsync(cancellationToken),
                        "lunch" => await restaurants.OfType<Lunch>().FirstOrDefaultAsync(cancellationToken),
                        "dinner" => await restaurants.OfType<Dinner>().FirstOrDefaultAsync(cancellationToken),
                        _ => await restaurants.OfType<PlaceAfterDinner>().FirstOrDefaultAsync(cancellationToken)
                    };

                case "firstplace":
                case "secondplace":
                case "thirdplace":
                case "place":
                    return await _context.Places
                        .Include(place => place.DisplayName)
                        .FirstOrDefaultAsync(place => place.GoogleId == googlePlaceId, cancellationToken);

                default:
                    throw new InvalidPlaceTypeException($"Invalid place type: {placeType}");
            }
        }

        private async Task<AppUser> RequireUserAsync(string username) =>
            await _userManager.FindByNameAsync(username)
            ?? throw new UserNotFoundException("User not found", UserNotFoundCode);
    }
}
