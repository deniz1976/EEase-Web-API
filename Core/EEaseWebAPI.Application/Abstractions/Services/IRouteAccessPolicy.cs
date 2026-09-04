using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRouteAccessPolicy
    {
        Task<RouteAccessResult> EvaluateAsync(
            StandardRoute route,
            string requesterUsername,
            string requesterUserId);

        Task<StandardRouteDTO> ToDtoAsync(
            StandardRoute route,
            string requesterUsername,
            string requesterUserId);
    }
}
