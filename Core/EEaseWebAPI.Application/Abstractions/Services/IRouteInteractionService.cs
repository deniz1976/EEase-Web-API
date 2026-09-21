using EEaseWebAPI.Application.Features.Commands.Place.LikePlace;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRouteInteractionService
    {
        Task<int> LikeRouteAsync(string username, Guid routeId, CancellationToken cancellationToken = default);

        Task<int> UnlikeRouteAsync(string username, Guid routeId, CancellationToken cancellationToken = default);

        Task<bool> DeleteRoute(string username, Guid? routeId, CancellationToken cancellationToken = default);

        Task<string> DeleteAllRoutes(string username, CancellationToken cancellationToken = default);

        Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username, CancellationToken cancellationToken = default);

        Task<LikePlaceCommandResponseBody> LikePlaceAsync(
            string username,
            string googlePlaceId,
            string placeType,
            CancellationToken cancellationToken = default);
    }
}
