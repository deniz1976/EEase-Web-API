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
            PRICE_LEVEL? _PRICE_LEVEL)
        {
            var route = await _randomRouteBuilder.BuildAsync(destination, startDate, endDate, _PRICE_LEVEL);

            return await SaveAsync(route);
        }

        public async Task<StandardRoute> CreatePrefRoute(
            string? destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? price_level,
            string? username,
            List<string>? friends)
        {
            var route = await _preferenceRouteBuilder.BuildAsync(
                destination, startDate, endDate, price_level, username, friends);

            return await SaveAsync(route);
        }

        private async Task<StandardRoute> SaveAsync(StandardRoute route)
        {
            await _context.StandardRoutes.AddAsync(route);
            await _context.SaveChangesAsync();

            // The owner is already known to the caller and would only bloat the response.
            route.User = null;

            return route;
        }
    }
}
