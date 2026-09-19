using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Handles a disliked place: the preferences it matched are pulled down, the place is
    /// never suggested to that user again, and the route gets a replacement in its slot.
    /// </summary>
    public interface IRouteDislikeService
    {
        Task<StandardRoute> DislikePlaceOrRestaurant(DislikePlaceOrRestaurantCommandRequest request);
    }
}
