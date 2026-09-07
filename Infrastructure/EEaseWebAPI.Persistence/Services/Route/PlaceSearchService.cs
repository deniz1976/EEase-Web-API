using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class PlaceSearchService : IPlaceSearchService
    {
        private static readonly HashSet<string> ExcludedPlaceTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "shopping_mall",
            "supermarket",
            "department_store",
            "grocery_or_supermarket",
            "convenience_store",
            "store",
            "home_goods_store",
            "furniture_store",
            "electronics_store",
            "hardware_store",
            "clothing_store",
            "gas_station",
            "car_dealer",
            "car_rental",
            "car_repair",
            "car_wash",
            "parking",
            "pharmacy",
            "drugstore",
            "laundry",
            "dry_cleaning",
            "locksmith",
            "real_estate_agency",
            "insurance_agency",
            "atm",
            "bank",
            "post_office",
            "school",
            "university",
            "hospital",
            "doctor",
            "dentist",
            "health",
            "physiotherapist",
            "moving_company",
            "storage",
            "accounting",
            "electrician",
            "plumber",
            "lawyer",
            "general_contractor",
            "roofing_contractor",
            "book_store",
            "shoe_store",
            "travel_agency",
            "tour_operator",
            "tourist_information_center",
            "travel_bureau",
            "tour_agency"
        };

        private readonly IGooglePlacesService _googlePlacesService;
        private readonly ILogger<PlaceSearchService> _logger;

        public PlaceSearchService(IGooglePlacesService googlePlacesService, ILogger<PlaceSearchService> logger)
        {
            _googlePlacesService = googlePlacesService;
            _logger = logger;
        }

        public async Task<IReadOnlyList<Place>> SearchAsync(string query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Array.Empty<Place>();
            }

            cancellationToken.ThrowIfCancellationRequested();

            var response = await _googlePlacesService.SearchPlacesAsync(query);

            var places = (response?.Places ?? new List<Place>())
                .Where(place => place != null)
                .Where(place => !string.IsNullOrWhiteSpace(place.Id))
                .Where(IsWanted)
                .ToList();

            _logger.LogDebug("Place search for {Query} returned {Count} usable places.", query, places.Count);

            return places;
        }

        public async Task<IReadOnlyList<Place>> SearchFirstMatchAsync(
            IEnumerable<string> queries,
            CancellationToken cancellationToken = default)
        {
            foreach (var query in queries)
            {
                var places = await SearchAsync(query, cancellationToken);

                if (places.Count > 0)
                {
                    return places;
                }
            }

            return Array.Empty<Place>();
        }

        public async Task<IReadOnlyList<string>> CollectPlaceIdsAsync(
            IEnumerable<string> queries,
            int requiredCount = 0,
            IEnumerable<string>? excludedPlaceIds = null,
            CancellationToken cancellationToken = default)
        {
            var excluded = excludedPlaceIds == null
                ? new HashSet<string>()
                : new HashSet<string>(excludedPlaceIds);

            var collected = new List<string>();
            var seen = new HashSet<string>();

            foreach (var query in queries)
            {
                if (requiredCount > 0 && collected.Count >= requiredCount)
                {
                    break;
                }

                foreach (var place in await SearchAsync(query, cancellationToken))
                {
                    var id = place.Id!;

                    if (excluded.Contains(id) || !seen.Add(id))
                    {
                        continue;
                    }

                    collected.Add(id);
                }
            }

            return collected;
        }

        private static bool IsWanted(Place place) =>
            place.Types == null
            || place.Types.Count == 0
            || !place.Types.Any(ExcludedPlaceTypes.Contains);
    }
}
