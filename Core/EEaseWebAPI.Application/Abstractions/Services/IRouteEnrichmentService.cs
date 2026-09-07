using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRouteEnrichmentService
    {
        Task<bool> ApplyAsync(
            StandardRoute? route,
            DateOnly? startDate,
            DateOnly? endDate,
            CancellationToken cancellationToken = default);
    }
}
