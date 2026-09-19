using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Builds a route from the merged preferences of the requesting user and the friends
    /// travelling with them, skipping places that user has disliked before.
    /// The returned route is not persisted.
    /// </summary>
    public interface IPreferenceRouteBuilder
    {
        Task<StandardRoute> BuildAsync(
            string? destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            string? username,
            List<string>? friends,
            CancellationToken cancellationToken = default);
    }
}
