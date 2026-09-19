using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Persistence.Services.Route
{
    /// <summary>
    /// The single entry point the route handlers talk to. It owns no logic of its own:
    /// reads go to <see cref="IRouteQueryService"/>, writes to
    /// <see cref="IRouteInteractionService"/> and dislikes to <see cref="IRouteDislikeService"/>.
    /// </summary>
    public sealed class RouteService : IRouteService
    {
        private readonly IRouteQueryService _routeQueryService;
        private readonly IRouteInteractionService _routeInteractionService;
        private readonly IRouteDislikeService _routeDislikeService;

        public RouteService(
            IRouteQueryService routeQueryService,
            IRouteInteractionService routeInteractionService,
            IRouteDislikeService routeDislikeService)
        {
            _routeQueryService = routeQueryService;
            _routeInteractionService = routeInteractionService;
            _routeDislikeService = routeDislikeService;
        }

        public Task<(List<StandardRoute> Routes, int TotalCount)> GetAllRoutes(
            string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
            _routeQueryService.GetAllRoutes(username, pageNumber, pageSize, cancellationToken);

        public Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetRoutesByUserId(
            string userId, string requesterUsername, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
            _routeQueryService.GetRoutesByUserId(userId, requesterUsername, pageNumber, pageSize, cancellationToken);

        public Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(
            string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default) =>
            _routeQueryService.GetLikedRoutes(username, pageNumber, pageSize, cancellationToken);

        public Task<StandardRouteDTO> GetRouteById(string username, Guid? routeId) =>
            _routeQueryService.GetRouteById(username, routeId);

        public Task<bool> CheckRouteLikeStatus(string username, Guid routeId) =>
            _routeQueryService.CheckRouteLikeStatus(username, routeId);

        public Task<bool> LikeRoute(string username, Guid routeId) =>
            _routeInteractionService.LikeRoute(username, routeId);

        public Task<bool> DeleteRoute(string username, Guid? routeId) =>
            _routeInteractionService.DeleteRoute(username, routeId);

        public Task<string> DeleteAllRoutes(string username) =>
            _routeInteractionService.DeleteAllRoutes(username);

        public Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username) =>
            _routeInteractionService.UpdateRouteStatusAsync(routeId, status, username);

        public Task<LikePlaceOrRestaurantCommandResponseBody> LikePlaceOrRestaurantAsync(
            string username, string googlePlaceId, string placeType) =>
            _routeInteractionService.LikePlaceOrRestaurantAsync(username, googlePlaceId, placeType);

        public Task<StandardRoute> DislikePlaceOrRestaurant(DislikePlaceOrRestaurantCommandRequest request) =>
            _routeDislikeService.DislikePlaceOrRestaurant(request);
    }
}
