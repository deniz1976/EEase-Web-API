using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// The things a user does to a route that already exists: liking it, changing who can
    /// see it, deleting it, and telling the system a single place was a good pick.
    /// </summary>
    public interface IRouteInteractionService
    {
        Task<bool> LikeRoute(string username, Guid routeId);

        Task<bool> DeleteRoute(string username, Guid? routeId);

        Task<string> DeleteAllRoutes(string username);

        Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username);

        Task<LikePlaceOrRestaurantCommandResponseBody> LikePlaceOrRestaurantAsync(
            string username,
            string googlePlaceId,
            string placeType,
            CancellationToken cancellationToken = default);
    }
}
