using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Builds a route that belongs to no one in particular: places are picked at random
    /// within the requested price level, with no user preferences involved.
    /// The returned route is not persisted.
    /// </summary>
    public interface IRandomRouteBuilder
    {
        Task<StandardRoute> BuildAsync(
            string destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            CancellationToken cancellationToken = default);
    }
}
