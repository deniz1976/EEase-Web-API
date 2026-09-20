using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.Features.Commands.Place.DislikePlace;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Queries;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using UserNotFoundException = EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class RouteDislikeService : IRouteDislikeService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EEaseAPIDbContext _context;
        private readonly IPreferenceProfileBuilder _preferenceProfileBuilder;
        private readonly IPreferenceFeedbackService _preferenceFeedbackService;
        private readonly IPlaceReplacementService _placeReplacementService;
        private readonly IDislikedPlaceService _dislikedPlaceService;
        private readonly IRouteEnrichmentService _routeEnrichmentService;
        private readonly ILogger<RouteDislikeService> _logger;

        public RouteDislikeService(
            UserManager<AppUser> userManager,
            EEaseAPIDbContext context,
            IPreferenceProfileBuilder preferenceProfileBuilder,
            IPreferenceFeedbackService preferenceFeedbackService,
            IPlaceReplacementService placeReplacementService,
            IDislikedPlaceService dislikedPlaceService,
            IRouteEnrichmentService routeEnrichmentService,
            ILogger<RouteDislikeService> logger)
        {
            _userManager = userManager;
            _context = context;
            _preferenceProfileBuilder = preferenceProfileBuilder;
            _preferenceFeedbackService = preferenceFeedbackService;
            _placeReplacementService = placeReplacementService;
            _dislikedPlaceService = dislikedPlaceService;
            _routeEnrichmentService = routeEnrichmentService;
            _logger = logger;
        }

        public async Task<StandardRoute> DislikePlaceAsync(
            DislikePlaceCommandRequest request,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(request.PlaceType))
                throw new InvalidPlaceTypeException("Place type must be specified");

            if (string.IsNullOrWhiteSpace(request.GooglePlaceId))
                throw new InvalidPlaceTypeException("Google place id must be specified");

            if (!Guid.TryParse(request.RouteId, out var routeId))
                throw new RouteNotFoundException("Route not found", (int)StatusEnum.RouteNotFound);

            var user = await _userManager.FindByNameAsync(request.Username)
                ?? throw new UserNotFoundException("User not found", (int)StatusEnum.UserNotFound);

            var route = await _context.StandardRoutes
                .IncludeFullRouteGraph()
                .IncludeRoutePreferences()
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId, cancellationToken)
                ?? throw new RouteNotFoundException("Route not found", (int)StatusEnum.RouteNotFound);

            if (route.UserId != user.Id)
                throw new ForbiddenException(
                    "You do not have permission to modify this route", StatusEnum.UnauthorizedToModifyRoute);

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var feedback = await _preferenceFeedbackService.ApplyAsync(
                    user.Id, LocatePlace(route, request.GooglePlaceId, request.PlaceType),
                    request.PlaceType, liked: false, cancellationToken);

                await _dislikedPlaceService.RecordAsync(
                    user.Id, request.GooglePlaceId, request.PlaceType, cancellationToken);

                await ReplacePlaceAsync(
                    route, user.Id, request.GooglePlaceId, request.PlaceType, cancellationToken);

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                _logger.LogInformation(
                    "Replaced a disliked {PlaceType} in route {Route}; preferences changed: {Changes}.",
                    request.PlaceType, route.Id, feedback.Describe());

                route.User = null;

                return route;
            }
            catch
            {
                // Deliberately not the request's token: a caller who gave up half way is the
                // reason we are here, and the rollback still has to happen.
                await transaction.RollbackAsync(CancellationToken.None);
                throw;
            }
        }

        private async Task ReplacePlaceAsync(
            StandardRoute route,
            string userId,
            string googlePlaceId,
            string placeType,
            CancellationToken cancellationToken)
        {
            var accommodationPreferences = await _context.UserAccommodationPreferences
                .FirstOrDefaultAsync(preference => preference.UserId == userId, cancellationToken);
            var foodPreferences = await _context.UserFoodPreferences
                .FirstOrDefaultAsync(preference => preference.UserId == userId, cancellationToken);
            var personalizationPreferences = await _context.UserPersonalizations
                .FirstOrDefaultAsync(preference => preference.UserId == userId, cancellationToken);

            var profile = _preferenceProfileBuilder.Build(
                accommodationPreferences, foodPreferences, personalizationPreferences);

            var disliked = await _dislikedPlaceService.GetGoogleIdsAsync(userId, cancellationToken);

            await _placeReplacementService.ReplaceAsync(
                route, profile, googlePlaceId, placeType, disliked, cancellationToken);

            // The replacement has no weather or travel time yet; enrichment reruns over the
            // whole route using the dates it was originally planned for.
            await _routeEnrichmentService.ApplyAsync(
                route,
                route.TravelDays.FirstOrDefault()?.Breakfast?.Weather?.Date,
                route.TravelDays.LastOrDefault()?.Breakfast?.Weather?.Date);
        }

        /// <summary>
        /// Finds the place inside the route by its Google id, whichever slot it sits in.
        /// </summary>
        private static BaseEntity LocatePlace(StandardRoute route, string googlePlaceId, string placeType)
        {
            foreach (var day in route.TravelDays)
            {
                BaseEntity?[] candidates =
                {
                    day.Accomodation, day.Breakfast, day.Lunch, day.Dinner,
                    day.PlaceAfterDinner, day.FirstPlace, day.SecondPlace, day.ThirdPlace
                };

                foreach (var candidate in candidates)
                {
                    var id = candidate switch
                    {
                        TravelAccomodation accommodation => accommodation.GoogleId,
                        BaseRestaurantPlaceEntity restaurant => restaurant.GoogleId,
                        BaseTravelPlaceEntity place => place.GoogleId,
                        _ => null
                    };

                    if (id == googlePlaceId)
                    {
                        return candidate!;
                    }
                }
            }

            throw new PlaceNotFoundInRouteException(
                $"No {placeType} with Google id {googlePlaceId} is part of this route.");
        }
    }
}
