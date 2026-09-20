using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Features.Commands.Place.DislikePlace;
using EEaseWebAPI.Application.Features.Commands.Place.LikePlace;
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

        public Task<StandardRouteDTO> GetRouteById(string username, Guid? routeId, CancellationToken cancellationToken = default) =>
            _routeQueryService.GetRouteById(username, routeId, cancellationToken);

        public Task<bool> CheckRouteLikeStatus(string username, Guid routeId, CancellationToken cancellationToken = default) =>
            _routeQueryService.CheckRouteLikeStatus(username, routeId, cancellationToken);

        public Task<bool> LikeRoute(string username, Guid routeId, CancellationToken cancellationToken = default) =>
            _routeInteractionService.LikeRoute(username, routeId, cancellationToken);

        public Task<bool> DeleteRoute(string username, Guid? routeId, CancellationToken cancellationToken = default) =>
            _routeInteractionService.DeleteRoute(username, routeId, cancellationToken);

        public Task<string> DeleteAllRoutes(string username, CancellationToken cancellationToken = default) =>
            _routeInteractionService.DeleteAllRoutes(username, cancellationToken);

        public Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(
            Guid routeId, int status, string username, CancellationToken cancellationToken = default) =>
            _routeInteractionService.UpdateRouteStatusAsync(routeId, status, username, cancellationToken);

        public Task<LikePlaceCommandResponseBody> LikePlaceAsync(
            string username,
            string googlePlaceId,
            string placeType,
            CancellationToken cancellationToken = default) =>
            _routeInteractionService.LikePlaceAsync(
                username, googlePlaceId, placeType, cancellationToken);

        public Task<StandardRoute> DislikePlaceAsync(
            DislikePlaceCommandRequest request,
            CancellationToken cancellationToken = default) =>
            _routeDislikeService.DislikePlaceAsync(request, cancellationToken);
    }
}
