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
using EEaseWebAPI.Application.Features.Commands.Place.LikePlace;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Application.Features.Commands.Place.DislikePlace;
using EEaseWebAPI.Application.DTOs.Place;
using EEaseWebAPI.Application.Enums;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRouteService
    {
        Task<(List<StandardRoute> Routes, int TotalCount)> GetAllRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetRoutesByUserId(string userId, string requesterUsername, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<bool> LikeRoute(string username, Guid routeId, CancellationToken cancellationToken = default);

        Task<bool> DeleteRoute(string username, Guid? routeId, CancellationToken cancellationToken = default);

        Task<StandardRouteDTO> GetRouteById(string username, Guid? routeId, CancellationToken cancellationToken = default);

        Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(
            Guid routeId, int status, string username, CancellationToken cancellationToken = default);

        Task<LikePlaceCommandResponseBody> LikePlaceAsync(
            string username,
            string googlePlaceId,
            string placeType,
            CancellationToken cancellationToken = default);

        Task<bool> CheckRouteLikeStatus(string username, Guid routeId, CancellationToken cancellationToken = default);

        Task<string> DeleteAllRoutes(string username, CancellationToken cancellationToken = default);

        Task<StandardRoute> DislikePlaceAsync(
            DislikePlaceCommandRequest dislikePlaceOrRestaurantDTO,
            CancellationToken cancellationToken = default);
    }
}
