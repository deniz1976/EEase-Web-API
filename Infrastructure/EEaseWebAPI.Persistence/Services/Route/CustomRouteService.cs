using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;

namespace EEaseWebAPI.Persistence.Services.Route
{
    /// <summary>
    /// Runs one of the two route builders and persists what it produced.
    /// The building itself lives in <see cref="IRandomRouteBuilder"/> and
    /// <see cref="IPreferenceRouteBuilder"/>.
    /// </summary>
    public sealed class CustomRouteService : ICustomRouteService
    {
        private readonly IRandomRouteBuilder _randomRouteBuilder;
        private readonly IPreferenceRouteBuilder _preferenceRouteBuilder;
        private readonly EEaseAPIDbContext _context;

        public CustomRouteService(
            IRandomRouteBuilder randomRouteBuilder,
            IPreferenceRouteBuilder preferenceRouteBuilder,
            EEaseAPIDbContext context)
        {
            _randomRouteBuilder = randomRouteBuilder;
            _preferenceRouteBuilder = preferenceRouteBuilder;
            _context = context;
        }

        public async Task<StandardRoute> CreateRandomRoute(
            string destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            CancellationToken cancellationToken = default)
        {
            var route = await _randomRouteBuilder.BuildAsync(
                destination, startDate, endDate, priceLevel, cancellationToken);

            return await SaveAsync(route, cancellationToken);
        }

        public async Task<StandardRoute> CreatePrefRoute(
            string? destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            string? username,
            List<string>? friends,
            CancellationToken cancellationToken = default)
        {
            var route = await _preferenceRouteBuilder.BuildAsync(
                destination, startDate, endDate, priceLevel, username, friends, cancellationToken);

            return await SaveAsync(route, cancellationToken);
        }

        private async Task<StandardRoute> SaveAsync(StandardRoute route, CancellationToken cancellationToken)
        {
            // Building a route takes minutes of calls to Gemini and Google. A caller who
            // gave up used to pay for all of them: the builders took a cancellation token
            // and nobody passed one in.
            await _context.StandardRoutes.AddAsync(route, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            // The owner is already known to the caller and would only bloat the response.
            route.User = null;

            return route;
        }
    }
}
