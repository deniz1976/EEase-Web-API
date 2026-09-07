using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class PlaceReplacementService : IPlaceReplacementService
    {
        private const int CandidatePoolSize = 8;

        private readonly IPlaceSearchService _placeSearchService;
        private readonly IPlaceSelectionService _placeSelectionService;
        private readonly IPlaceQueryBuilder _placeQueryBuilder;
        private readonly ILogger<PlaceReplacementService> _logger;
        private readonly Random _random;

        public PlaceReplacementService(
            IPlaceSearchService placeSearchService,
            IPlaceSelectionService placeSelectionService,
            IPlaceQueryBuilder placeQueryBuilder,
            ILogger<PlaceReplacementService> logger,
            Random random)
        {
            _placeSearchService = placeSearchService;
            _placeSelectionService = placeSelectionService;
            _placeQueryBuilder = placeQueryBuilder;
            _logger = logger;
            _random = random;
        }

        public async Task<TravelDay> ReplaceAsync(
            StandardRoute route,
            PreferenceProfile profile,
            string googlePlaceId,
            string placeType,
            IReadOnlyCollection<string> alsoExclude,
            CancellationToken cancellationToken = default)
        {
            var (day, slot) = Locate(route, googlePlaceId, placeType);

            var destination = route.City
                ?? throw new RouteGenerationException("The route has no destination to search in.");

            var priceLevel = route.TravelDays
                .Select(travelDay => travelDay.Accomodation?._PRICE_LEVEL)
                .FirstOrDefault(level => level != null) ?? PRICE_LEVEL.PRICE_LEVEL_MODERATE;

            var excluded = new HashSet<string>(UsedGoogleIds(route));
            excluded.Add(googlePlaceId);
            excluded.UnionWith(alsoExclude);

            var (label, queries) = Queries(slot, profile, priceLevel, destination);

            var candidates = await _placeSearchService.CollectPlaceIdsAsync(
                queries, CandidatePoolSize, excluded, cancellationToken);

            if (candidates.Count == 0)
            {
                throw new RouteGenerationException(
                    $"No unused alternative could be found for {slot} in {destination}.");
            }

            var chosen = candidates[_random.Next(candidates.Count)];

            _logger.LogInformation(
                "Replacing {Slot} {Old} with {New} in route {Route}.", slot, googlePlaceId, chosen, route.Id);

            await SwapAsync(route, day, slot, chosen, label, priceLevel, cancellationToken);

            return day;
        }

        private static (TravelDay Day, PlaceSlot Slot) Locate(
            StandardRoute route, string googlePlaceId, string placeType)
        {
            var wanted = Slots(placeType);

            foreach (var day in route.TravelDays)
            {
                foreach (var slot in wanted)
                {
                    if (GoogleIdOf(day, slot) == googlePlaceId)
                    {
                        return (day, slot);
                    }
                }
            }

            throw new PlaceNotFoundInRouteException(
                $"No {placeType} with Google id {googlePlaceId} is part of this route.");
        }

        private static PlaceSlot[] Slots(string placeType) => placeType.ToLowerInvariant() switch
        {
            "accommodation" => new[] { PlaceSlot.Accommodation },
            "breakfast" => new[] { PlaceSlot.Breakfast },
            "lunch" => new[] { PlaceSlot.Lunch },
            "dinner" => new[] { PlaceSlot.Dinner },
            "placeafterdinner" => new[] { PlaceSlot.AfterDinner },
            "place" or "firstplace" or "secondplace" or "thirdplace" =>
                new[] { PlaceSlot.FirstPlace, PlaceSlot.SecondPlace, PlaceSlot.ThirdPlace },
            _ => throw new InvalidPlaceTypeException($"Invalid place type: {placeType}")
        };

        private static string? GoogleIdOf(TravelDay day, PlaceSlot slot) => slot switch
        {
            PlaceSlot.Accommodation => day.Accomodation?.GoogleId,
            PlaceSlot.Breakfast => day.Breakfast?.GoogleId,
            PlaceSlot.Lunch => day.Lunch?.GoogleId,
            PlaceSlot.Dinner => day.Dinner?.GoogleId,
            PlaceSlot.AfterDinner => day.PlaceAfterDinner?.GoogleId,
            PlaceSlot.FirstPlace => day.FirstPlace?.GoogleId,
            PlaceSlot.SecondPlace => day.SecondPlace?.GoogleId,
            _ => day.ThirdPlace?.GoogleId
        };

        public static IEnumerable<string> UsedGoogleIds(StandardRoute route)
        {
            foreach (var day in route.TravelDays)
            {
                foreach (var slot in Enum.GetValues<PlaceSlot>())
                {
                    var googleId = GoogleIdOf(day, slot);

                    if (googleId != null)
                    {
                        yield return googleId;
                    }
                }
            }
        }

        private (string Label, IReadOnlyList<string> Queries) Queries(
            PlaceSlot slot, PreferenceProfile profile, PRICE_LEVEL? priceLevel, string destination)
        {
            switch (slot)
            {
                case PlaceSlot.Accommodation:
                    var accommodation = _placeQueryBuilder.Accommodation(profile.Accommodation, priceLevel);

                    return (accommodation, In(destination,
                        accommodation,
                        $"{_placeQueryBuilder.HotelStars(priceLevel)} star hotel",
                        "hotel"));

                case PlaceSlot.Breakfast:
                case PlaceSlot.Lunch:
                case PlaceSlot.Dinner:
                    var mealType = slot switch
                    {
                        PlaceSlot.Breakfast => MealType.Breakfast,
                        PlaceSlot.Lunch => MealType.Lunch,
                        _ => MealType.Dinner
                    };

                    var meal = _placeQueryBuilder.Food(profile.Food, priceLevel, mealType);

                    return (meal, In(destination,
                        meal,
                        _placeQueryBuilder.Food(null, priceLevel, mealType),
                        _placeQueryBuilder.AlternativeFood(mealType, priceLevel),
                        "restaurant"));

                case PlaceSlot.AfterDinner:
                    var afterDinner = _placeQueryBuilder.AfterDinner(profile.Personalization, priceLevel);

                    return (afterDinner, In(destination,
                        afterDinner,
                        _placeQueryBuilder.AlternativeAfterDinner(priceLevel),
                        _placeQueryBuilder.AlternativeAfterDinner(priceLevel),
                        "evening entertainment"));

                default:
                    var touristic = _placeQueryBuilder.Touristic(profile.Personalization);

                    return (touristic, In(destination,
                        touristic,
                        _placeQueryBuilder.AlternativeTouristic(profile.Personalization, touristic),
                        "hidden gems",
                        "famous tourist attractions"));
            }
        }

        private static IReadOnlyList<string> In(string destination, params string[] queries) =>
            queries
                .Where(query => !string.IsNullOrWhiteSpace(query))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .Select(query => $"{query} in {destination}")
                .ToList();

        private async Task SwapAsync(
            StandardRoute route,
            TravelDay day,
            PlaceSlot slot,
            string googleId,
            string label,
            PRICE_LEVEL? priceLevel,
            CancellationToken cancellationToken)
        {
            switch (slot)
            {
                case PlaceSlot.Accommodation:
                    var accommodation = await _placeSelectionService
                        .MaterializeAsync<TravelAccomodation>(googleId, priceLevel, cancellationToken);

                    accommodation.UserAccomodationPreference = label;
                    accommodation.Star = _placeQueryBuilder.HotelStars(priceLevel);

                    foreach (var travelDay in route.TravelDays)
                    {
                        travelDay.Accomodation = Copy(accommodation);
                    }

                    break;

                case PlaceSlot.Breakfast:
                    day.Breakfast = await MaterializeMealAsync<Breakfast>(
                        googleId, label, priceLevel, day.Breakfast, cancellationToken);
                    break;

                case PlaceSlot.Lunch:
                    day.Lunch = await MaterializeMealAsync<Lunch>(
                        googleId, label, priceLevel, day.Lunch, cancellationToken);
                    break;

                case PlaceSlot.Dinner:
                    day.Dinner = await MaterializeMealAsync<Dinner>(
                        googleId, label, priceLevel, day.Dinner, cancellationToken);
                    break;

                case PlaceSlot.AfterDinner:
                    day.PlaceAfterDinner = await MaterializeMealAsync<PlaceAfterDinner>(
                        googleId, label, priceLevel, day.PlaceAfterDinner, cancellationToken);
                    break;

                case PlaceSlot.FirstPlace:
                    day.FirstPlace = await MaterializePlaceAsync(
                        googleId, label, day.FirstPlace, cancellationToken);
                    break;

                case PlaceSlot.SecondPlace:
                    day.SecondPlace = await MaterializePlaceAsync(
                        googleId, label, day.SecondPlace, cancellationToken);
                    break;

                default:
                    day.ThirdPlace = await MaterializePlaceAsync(
                        googleId, label, day.ThirdPlace, cancellationToken);
                    break;
            }
        }

        private async Task<T> MaterializeMealAsync<T>(
            string googleId,
            string label,
            PRICE_LEVEL? priceLevel,
            T? previous,
            CancellationToken cancellationToken)
            where T : BaseRestaurantPlaceEntity, ISelectablePlace, new()
        {
            var place = await _placeSelectionService.MaterializeAsync<T>(googleId, priceLevel, cancellationToken);

            place.UserFoodPreference = label;
            place.Weather = previous?.Weather;

            return place;
        }

        private async Task<Domain.Entities.Route.Place> MaterializePlaceAsync(
            string googleId,
            string label,
            Domain.Entities.Route.Place? previous,
            CancellationToken cancellationToken)
        {
            var place = await _placeSelectionService
                .MaterializeAsync<Domain.Entities.Route.Place>(googleId, null, cancellationToken);

            place.UserPersonalizationPref = label;
            place.Weather = previous?.Weather;

            return place;
        }

        private static TravelAccomodation Copy(TravelAccomodation source) => new()
        {
            Id = Guid.NewGuid(),
            _PRICE_LEVEL = source._PRICE_LEVEL,
            DisplayName = source.DisplayName,
            FormattedAddress = source.FormattedAddress,
            GoogleId = source.GoogleId,
            GoogleMapsUri = source.GoogleMapsUri,
            Location = source.Location,
            NationalPhoneNumber = source.NationalPhoneNumber,
            Photos = source.Photos,
            PrimaryType = source.PrimaryType,
            Rating = source.Rating,
            RegularOpeningHours = source.RegularOpeningHours,
            Restroom = source.Restroom,
            WebsiteUri = source.WebsiteUri,
            Star = source.Star,
            UserAccomodationPreference = source.UserAccomodationPreference
        };
    }
}
