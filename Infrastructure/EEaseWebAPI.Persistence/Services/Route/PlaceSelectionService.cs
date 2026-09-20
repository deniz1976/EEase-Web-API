using System.Collections.Concurrent;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class PlaceSelectionService : IPlaceSelectionService
    {
        /// <summary>
        /// What Google said about a place, for as long as this request lasts. A place is
        /// asked about more than once: the same hotel is copied onto every day, a rejected
        /// plan is built again from the same pool, and the details of a place do not change
        /// between those calls. The task is stored rather than the answer, so two slots
        /// asking at the same time make one call between them.
        /// </summary>
        private readonly ConcurrentDictionary<string, Task<string>> _details = new();

        private readonly IGooglePlacesService _googlePlacesService;
        private readonly ILogger<PlaceSelectionService> _logger;

        public PlaceSelectionService(
            IGooglePlacesService googlePlacesService,
            ILogger<PlaceSelectionService> logger)
        {
            _googlePlacesService = googlePlacesService;
            _logger = logger;
        }

        public async Task<T> SelectAsync<T>(
            IReadOnlyList<string> pool,
            PlacePicker picker,
            PRICE_LEVEL? priceLevel = null,
            int? offset = null,
            CancellationToken cancellationToken = default)
            where T : class, ISelectablePlace, new()
        {
            ArgumentNullException.ThrowIfNull(picker);

            var googleId = offset.HasValue ? picker.TakeAt(pool, offset.Value) : picker.Take(pool);

            if (googleId == null)
            {
                throw new InvalidOperationException(
                    $"The pool of {pool?.Count ?? 0} places holds no unused entry for {typeof(T).Name}.");
            }

            return await MaterializeAsync<T>(googleId, priceLevel, cancellationToken);
        }

        public async Task<IReadOnlyList<T>> SelectManyAsync<T>(
            IReadOnlyList<string> pool,
            PlacePicker picker,
            int count,
            PRICE_LEVEL? priceLevel = null,
            int? offset = null,
            CancellationToken cancellationToken = default)
            where T : class, ISelectablePlace, new()
        {
            var selected = new List<T>(count);

            for (var index = 0; index < count; index++)
            {
                selected.Add(await SelectAsync<T>(
                    pool,
                    picker,
                    priceLevel,
                    offset.HasValue ? offset.Value + index : null,
                    cancellationToken));
            }

            return selected;
        }

        private async Task<string> DetailsOfAsync(string googleId, CancellationToken cancellationToken)
        {
            var pending = _details.GetOrAdd(
                googleId, id => _googlePlacesService.GetPlaceDetailsAsync(id, cancellationToken));

            try
            {
                return await pending;
            }
            catch
            {
                // A call that failed is not an answer worth keeping: the next slot that wants
                // this place should be allowed to ask again.
                _details.TryRemove(googleId, out _);
                throw;
            }
        }

        public async Task<T> MaterializeAsync<T>(
            string googleId,
            PRICE_LEVEL? priceLevel = null,
            CancellationToken cancellationToken = default)
            where T : class, ISelectablePlace, new()
        {
            if (string.IsNullOrWhiteSpace(googleId))
            {
                throw new InvalidOperationException($"A {typeof(T).Name} cannot be built without a place id.");
            }

            cancellationToken.ThrowIfCancellationRequested();

            var details = await DetailsOfAsync(googleId, cancellationToken);

            T? place;

            try
            {
                place = JsonConvert.DeserializeObject<T>(details);
            }
            catch (JsonException exception)
            {
                throw new InvalidOperationException(
                    $"The details of place {googleId} could not be read as {typeof(T).Name}.", exception);
            }

            if (place == null)
            {
                throw new InvalidOperationException($"The details of place {googleId} were empty.");
            }

            place.Id = Guid.NewGuid();
            place.GoogleId = googleId;
            place._PRICE_LEVEL = priceLevel;

            _logger.LogDebug("Built a {PlaceType} from place {GoogleId}.", typeof(T).Name, googleId);

            return place;
        }
    }
}
