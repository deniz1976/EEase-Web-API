using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRouteQueryService
    {
        Task<(List<StandardRoute> Routes, int TotalCount)> GetAllRoutes(
            string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetRoutesByUserId(
            string userId, string requesterUsername, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(
            string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default);

        Task<StandardRouteDTO> GetRouteById(
            string username, Guid? routeId, CancellationToken cancellationToken = default);

        Task<bool> CheckRouteLikeStatus(
            string username, Guid routeId, CancellationToken cancellationToken = default);
    }
}
