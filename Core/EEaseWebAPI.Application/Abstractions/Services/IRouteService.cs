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
        Task<(List<StandardRoute> Routes, int TotalCount)> GetAllRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetRoutesByUserId(string userId, string requesterUsername, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<bool> LikeRoute(string username, Guid routeId);

        Task<bool> DeleteRoute(string username, Guid? routeId);

        Task<StandardRouteDTO> GetRouteById(string username, Guid? routeId);

        Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username);

        Task<LikePlaceOrRestaurantCommandResponseBody> LikePlaceOrRestaurantAsync(
            string username,
            string googlePlaceId,
            string placeType);

        Task<bool> CheckRouteLikeStatus(string username, Guid routeId);

        Task<string> DeleteAllRoutes(string username);

        Task<StandardRoute> DislikePlaceOrRestaurant(DislikePlaceOrRestaurantCommandRequest dislikePlaceOrRestaurantDTO);
    }
}
