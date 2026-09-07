using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using System;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.DTOs.Route.DislikePlaceOrRestaurantDTO;
using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;
using System.Text;
using System.Linq;

using EEaseWebAPI.Persistence.Queries;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Application.DTOs.Route;

namespace EEaseWebAPI.Persistence.Services
{
    public class RouteService : IRouteService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IFriendshipService _friendshipService;
        private readonly EEaseAPIDbContext _context;
        private readonly IRouteAccessPolicy _routeAccessPolicy;
        private readonly IRouteEnrichmentService _routeEnrichmentService;
        private readonly IPreferenceProfileBuilder _preferenceProfileBuilder;
        private readonly IPreferenceFeedbackService _preferenceFeedbackService;
        private readonly IPlaceReplacementService _placeReplacementService;
        private readonly IDislikedPlaceService _dislikedPlaceService;
        private readonly ILogger<RouteService> _logger;

        public RouteService(
            UserManager<AppUser> userManager,
            IFriendshipService friendshipService,
            EEaseAPIDbContext context,
            IRouteAccessPolicy routeAccessPolicy,
            IRouteEnrichmentService routeEnrichmentService,
            IPreferenceProfileBuilder preferenceProfileBuilder,
            IPreferenceFeedbackService preferenceFeedbackService,
            IPlaceReplacementService placeReplacementService,
            IDislikedPlaceService dislikedPlaceService,
            ILogger<RouteService> logger)
        {
            _userManager = userManager;
            _friendshipService = friendshipService;
            _context = context;
            _routeAccessPolicy = routeAccessPolicy;
            _routeEnrichmentService = routeEnrichmentService;
            _preferenceProfileBuilder = preferenceProfileBuilder;
            _preferenceFeedbackService = preferenceFeedbackService;
            _placeReplacementService = placeReplacementService;
            _dislikedPlaceService = dislikedPlaceService;
            _logger = logger;
        }

        public async Task<(List<StandardRoute> Routes, int TotalCount)> GetAllRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", 7);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .Where(route => route.UserId == user.Id)
                .OrderByDescending(route => route.CreatedDate);

            var totalCount = await query.CountAsync(cancellationToken);

            var routes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            foreach (var route in routes)
            {
                route.User = null;
                route.LikedUsers = null;
            }

            return (routes, totalCount);
        }

        public async Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", 7);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .Where(route => route.LikedUsers!.Any(liker => liker.Id == user.Id))
                .OrderByDescending(route => route.CreatedDate);

            var totalCount = await query.CountAsync(cancellationToken);

            var likedRoutes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new List<StandardRouteDTO>(likedRoutes.Count);

            foreach (var route in likedRoutes)
            {
                result.Add(await _routeAccessPolicy.ToDtoAsync(route, username, user.Id));
            }

            return (result, totalCount);
        }

        public async Task<bool> LikeRoute(string username, Guid routeId)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", 7);

            var route = await _context.StandardRoutes
                .Include(r => r.LikedUsers)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new ArgumentException("Route not found", nameof(routeId));

            var access = await _routeAccessPolicy.EvaluateAsync(route, username, user.Id);

            if (!access.IsAccessible)
                throw new UnauthorizedAccessException(access.Message);

            var isLiked = route.LikedUsers.Any(u => u.Id == user.Id);
            if (isLiked)
            {
                route.LikedUsers.Remove(user);
                route.LikeCount--;
            }
            else
            {
                route.LikedUsers.Add(user);
                route.LikeCount++;
            }

            await _context.SaveChangesAsync();
            return !isLiked;
        }

        public async Task<bool> DeleteRoute(string username, Guid? routeId)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", 7);

            var route = await _context.StandardRoutes
                .Include(r => r.TravelDays)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new RouteNotFoundException("Route not found", 93);

            if (route.UserId != user.Id)
                throw new DeleteRouteException("Unauthorized to delete route");

            _context.StandardRoutes.Remove(route);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<StandardRouteDTO> GetRouteById(string username, Guid? routeId)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user is null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

            var route = await _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId);

            if (route is null)
                throw new RouteNotFoundException("Route not found", (int)StatusEnum.RouteNotFound);

            return await _routeAccessPolicy.ToDtoAsync(route, username, user.Id);
        }

        public async Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

            var route = await _context.StandardRoutes
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new RouteNotFoundException("Route not found", 93);

            if (route.UserId != user.Id)
                throw new UnauthorizedAccessException("Only route owner can update route status");

            if (status < 0 || status > 2)
                throw new InvalidRouteStatusException();

            route.Status = status;
            await _context.SaveChangesAsync();

            return new UpdateRouteStatusCommandResponseBody
            {
                IsUpdated = true
            };
        }

        private async Task<BaseEntity> GetPlaceByGoogleId(string googlePlaceId, string placeType)
        {
            switch (placeType.ToLower())
            {
                case "accommodation":
                    return await _context.TravelAccomodations
                        .Include(p => p.DisplayName)
                        .FirstOrDefaultAsync(p => p.GoogleId == googlePlaceId);

                case "breakfast":
                case "lunch":
                case "dinner":
                case "placeafterdinner":
                    var restaurantQuery = _context.Set<BaseRestaurantPlaceEntity>()
                        .Include(p => p.DisplayName)
                        .Where(p => p.GoogleId == googlePlaceId);

                    return placeType.ToLower() switch
                    {
                        "breakfast" => await restaurantQuery.OfType<Breakfast>().FirstOrDefaultAsync(),
                        "lunch" => await restaurantQuery.OfType<Lunch>().FirstOrDefaultAsync(),
                        "dinner" => await restaurantQuery.OfType<Dinner>().FirstOrDefaultAsync(),
                        "placeafterdinner" => await restaurantQuery.OfType<PlaceAfterDinner>().FirstOrDefaultAsync(),
                        _ => null
                    };

                case "firstplace":
                case "secondplace":
                case "thirdplace":
                case "place":
                    return await _context.Places
                        .Include(p => p.DisplayName)
                        .FirstOrDefaultAsync(p => p.GoogleId == googlePlaceId);

                default:
                    throw new InvalidPlaceTypeException($"Invalid place type: {placeType}");
            }
        }

        public async Task<LikePlaceOrRestaurantCommandResponseBody> LikePlaceOrRestaurantAsync(
            string username,
            string googlePlaceId,
            string placeType)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

            var place = await GetPlaceByGoogleId(googlePlaceId, placeType.ToLower());
            if (place == null)
                throw new InvalidPlaceTypeException($"Place not found with Google ID: {googlePlaceId}");

            var feedback = await _preferenceFeedbackService.ApplyAsync(user.Id, place, placeType, liked: true);

            return new LikePlaceOrRestaurantCommandResponseBody
            {
                IsPreferenceUpdated = feedback.HasChanges,
                Message = $"{feedback.Category} preferences updated: {feedback.Describe()}"
            };
        }

        public async Task<bool> CheckRouteLikeStatus(string username, Guid routeId)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                    throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

                var route = await _context.StandardRoutes
                    .Include(r => r.User)
                    .Include(r => r.LikedUsers)
                    .FirstOrDefaultAsync(r => r.Id == routeId);

                if (route == null)
                    throw new BaseException("Route not found", (int)StatusEnum.RouteNotFound);

                var access = await _routeAccessPolicy.EvaluateAsync(route, username, user.Id);

                if (!access.IsAccessible)
                    throw new BaseException(access.Message!, (int)StatusEnum.UnauthorizedToViewRoute);

                return route.LikedUsers.Any(u => u.Id == user.Id);
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, (int)StatusEnum.UnknownError);
            }
        }

        public async Task<string> DeleteAllRoutes(string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);
            var routes = await _context.StandardRoutes
                .Where(r => r.UserId == user.Id)
                .ToListAsync();
            if (routes.Count == 0)
                return "route count is 0.";
            _context.StandardRoutes.RemoveRange(routes);
            await _context.SaveChangesAsync();
            return "routes deleted successfully.";
        }

        public async Task<StandardRoute> DislikePlaceOrRestaurant(DislikePlaceOrRestaurantCommandRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.PlaceType))
                throw new InvalidPlaceTypeException("Place type must be specified");

            if (string.IsNullOrWhiteSpace(request.GooglePlaceId))
                throw new InvalidPlaceTypeException("Google place id must be specified");

            if (!Guid.TryParse(request.RouteId, out var routeId))
                throw new RouteNotFoundException("Route not found", (int)StatusEnum.RouteNotFound);

            var user = await _userManager.FindByNameAsync(request.Username)
                ?? throw new Application.Exceptions.Login.UserNotFoundException(
                    "User not found", (int)StatusEnum.UserNotFound);

            var route = await _context.StandardRoutes
                .IncludeFullRouteGraph()
                .IncludeRoutePreferences()
                .FirstOrDefaultAsync(candidate => candidate.Id == routeId)
                ?? throw new RouteNotFoundException("Route not found", (int)StatusEnum.RouteNotFound);

            if (route.UserId != user.Id)
                throw new UnauthorizedAccessException("You do not have permission to modify this route");

            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var removed = await _preferenceFeedbackService.ApplyAsync(
                    user.Id, LocatePlace(route, request.GooglePlaceId, request.PlaceType),
                    request.PlaceType, liked: false);

                await _dislikedPlaceService.RecordAsync(user.Id, request.GooglePlaceId, request.PlaceType);

                var userAccommodationPrefs = await _context.UserAccommodationPreferences
                    .FirstOrDefaultAsync(preference => preference.UserId == user.Id);
                var userFoodPrefs = await _context.UserFoodPreferences
                    .FirstOrDefaultAsync(preference => preference.UserId == user.Id);
                var userPersonalizationPrefs = await _context.UserPersonalizations
                    .FirstOrDefaultAsync(preference => preference.UserId == user.Id);

                var profile = _preferenceProfileBuilder.Build(
                    userAccommodationPrefs, userFoodPrefs, userPersonalizationPrefs);

                var disliked = await _dislikedPlaceService.GetGoogleIdsAsync(user.Id);

                await _placeReplacementService.ReplaceAsync(
                    route, profile, request.GooglePlaceId, request.PlaceType, disliked);

                await _routeEnrichmentService.ApplyAsync(
                    route,
                    route.TravelDays.FirstOrDefault()?.Breakfast?.Weather?.Date,
                    route.TravelDays.LastOrDefault()?.Breakfast?.Weather?.Date);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation(
                    "Replaced a disliked {PlaceType} in route {Route}; preferences changed: {Changes}.",
                    request.PlaceType, route.Id, removed.Describe());

                route.User = null;
                return route;
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

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

        public async Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetRoutesByUserId(string userId, string requesterUsername, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var targetUser = await _userManager.FindByIdAsync(userId);
            if (targetUser is null)
                throw new Application.Exceptions.Login.UserNotFoundException("Target user not found", 7);

            var requester = await _userManager.FindByNameAsync(requesterUsername);
            if (requester is null)
                throw new Application.Exceptions.Login.UserNotFoundException("Requester not found", 7);

            var isOwnProfile = targetUser.Id == requester.Id;

            var areFriends = !isOwnProfile
                && await _friendshipService.AreFriendsAsync(targetUser.UserName!, requesterUsername);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .IncludeFullRouteGraph()
                .Where(route => route.UserId == targetUser.Id);

            if (!isOwnProfile)
            {
                query = query.Where(route =>
                    route.Status == (int)RouteVisibility.Public
                    || (route.Status == (int)RouteVisibility.FriendsOnly && areFriends));
            }

            var orderedQuery = query.OrderByDescending(route => route.CreatedDate);

            var totalCount = await orderedQuery.CountAsync(cancellationToken);

            var routes = await orderedQuery
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new List<StandardRouteDTO>(routes.Count);

            foreach (var route in routes)
            {
                result.Add(await _routeAccessPolicy.ToDtoAsync(route, requesterUsername, requester.Id));
            }

            return (result, totalCount);
        }
    }
}
