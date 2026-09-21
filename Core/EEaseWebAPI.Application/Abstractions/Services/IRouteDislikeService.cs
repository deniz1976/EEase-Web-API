using EEaseWebAPI.Application.Features.Commands.Place.DislikePlace;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRouteDislikeService
    {
        Task<StandardRoute> DislikePlaceAsync(
            DislikePlaceCommandRequest request,
            CancellationToken cancellationToken = default);
    }
}
