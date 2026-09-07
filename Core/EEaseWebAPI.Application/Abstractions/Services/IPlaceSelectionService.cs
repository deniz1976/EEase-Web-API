using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPlaceSelectionService
    {
        Task<T> SelectAsync<T>(
            IReadOnlyList<string> pool,
            PlacePicker picker,
            PRICE_LEVEL? priceLevel = null,
            int? offset = null,
            CancellationToken cancellationToken = default)
            where T : class, ISelectablePlace, new();

        Task<IReadOnlyList<T>> SelectManyAsync<T>(
            IReadOnlyList<string> pool,
            PlacePicker picker,
            int count,
            PRICE_LEVEL? priceLevel = null,
            int? offset = null,
            CancellationToken cancellationToken = default)
            where T : class, ISelectablePlace, new();

        Task<T> MaterializeAsync<T>(
            string googleId,
            PRICE_LEVEL? priceLevel = null,
            CancellationToken cancellationToken = default)
            where T : class, ISelectablePlace, new();
    }
}
