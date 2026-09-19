using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Domain.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class PreferenceRouteBuilder : IPreferenceRouteBuilder
    {
        private const int MaxDayRetries = 3;
        private const int TouristicPoolPerDay = 5;
        private const int AfterDinnerPoolPerDay = 2;

        private readonly UserManager<AppUser> _userManager;
        private readonly IPlaceSearchService _placeSearchService;
        private readonly IPlaceSelectionService _placeSelectionService;
        private readonly IPlaceQueryBuilder _placeQueryBuilder;
        private readonly IPreferenceProfileBuilder _preferenceProfileBuilder;
        private readonly ITravellerPreferenceCollector _travellerPreferenceCollector;
        private readonly IRouteEnrichmentService _routeEnrichmentService;
        private readonly IRoutePlanValidator _routePlanValidator;
        private readonly IDislikedPlaceService _dislikedPlaceService;
        private readonly ILogger<PreferenceRouteBuilder> _logger;
        private readonly Random _random;

        public PreferenceRouteBuilder(
            UserManager<AppUser> userManager,
            IPlaceSearchService placeSearchService,
            IPlaceSelectionService placeSelectionService,
            IPlaceQueryBuilder placeQueryBuilder,
            IPreferenceProfileBuilder preferenceProfileBuilder,
            ITravellerPreferenceCollector travellerPreferenceCollector,
            IRouteEnrichmentService routeEnrichmentService,
            IRoutePlanValidator routePlanValidator,
            IDislikedPlaceService dislikedPlaceService,
            ILogger<PreferenceRouteBuilder> logger,
            Random random)
        {
            _userManager = userManager;
            _placeSearchService = placeSearchService;
            _placeSelectionService = placeSelectionService;
            _placeQueryBuilder = placeQueryBuilder;
            _preferenceProfileBuilder = preferenceProfileBuilder;
            _travellerPreferenceCollector = travellerPreferenceCollector;
            _routeEnrichmentService = routeEnrichmentService;
            _routePlanValidator = routePlanValidator;
            _dislikedPlaceService = dislikedPlaceService;
            _logger = logger;
            _random = random;
        }

        public async Task<StandardRoute> BuildAsync(
            string? destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            string? username,
            List<string>? friends,
            CancellationToken cancellationToken = default)
        {
            RouteBuilding.ValidateParameters(destination, startDate, endDate);

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));

            var city = RouteBuilding.FormatDestination(destination);
            var dayCount = RouteBuilding.CalculateDayCount(startDate, endDate);

            var user = await _userManager.FindByNameAsync(username)
                ?? throw new UserNotFoundException("User not found");

            priceLevel ??= PRICE_LEVEL.PRICE_LEVEL_MODERATE;

            var travellerPreferences = await _travellerPreferenceCollector.CollectAsync(user, friends, cancellationToken);
            var profile = _preferenceProfileBuilder.Build(travellerPreferences);

            var standardRoute = RouteBuilding.InitializeStandardRoute(city, dayCount, user.Id);

            var dislikedGoogleIds = await _dislikedPlaceService.GetGoogleIdsAsync(user.Id, cancellationToken);

            var accommodation = await FindHotelAsync(city, priceLevel, profile, dislikedGoogleIds);

            foreach (var travelDay in standardRoute.TravelDays)
            {
                travelDay.Accomodation = RouteBuilding.CopyAccommodation(accommodation, priceLevel);
            }

            // The picker starts out holding every disliked place, so they are never handed back.
            var picker = new PlacePicker(_random, dislikedGoogleIds);

            await FillMealsAsync(standardRoute, city, priceLevel, profile, picker, dayCount, cancellationToken);

            var touristicQuery = _placeQueryBuilder.Touristic(profile.Personalization);
            var touristicGoogleIds = await BuildTouristicPoolAsync(city, touristicQuery, profile, dayCount, picker);

            var afterDinnerQuery = _placeQueryBuilder.AfterDinner(profile.Personalization, priceLevel);
            var afterDinnerGoogleIds = await BuildAfterDinnerPoolAsync(city, afterDinnerQuery, picker);

            if (touristicGoogleIds.Count < dayCount * RouteBuilding.TouristicPlacesPerDay ||
                afterDinnerGoogleIds.Count < dayCount)
            {
                await WidenPoolsAsync(city, dayCount, touristicGoogleIds, afterDinnerGoogleIds, picker);
            }

            for (int day = 0; day < dayCount; day++)
            {
                await FillSightseeingAsync(
                    standardRoute, day, city, priceLevel, picker,
                    touristicGoogleIds, touristicQuery,
                    afterDinnerGoogleIds, afterDinnerQuery,
                    cancellationToken);
            }

            var validation = _routePlanValidator.Validate(standardRoute);

            if (!validation.IsValid)
            {
                throw new RouteGenerationException(
                    $"No usable route could be built for {city} ({validation.Describe()}).");
            }

            await _routeEnrichmentService.ApplyAsync(standardRoute, startDate, endDate);

            return standardRoute;
        }

        private async Task FillMealsAsync(
            StandardRoute route,
            string destination,
            PRICE_LEVEL? priceLevel,
            PreferenceProfile profile,
            PlacePicker picker,
            int dayCount,
            CancellationToken cancellationToken)
        {
            for (int day = 0; day < dayCount; day++)
            {
                route.TravelDays[day].Breakfast = await SelectMealAsync<Breakfast>(
                    destination, priceLevel, profile, MealType.Breakfast, day, picker, cancellationToken);

                route.TravelDays[day].Lunch = await SelectMealAsync<Lunch>(
                    destination, priceLevel, profile, MealType.Lunch, day, picker, cancellationToken);

                route.TravelDays[day].Dinner = await SelectMealAsync<Dinner>(
                    destination, priceLevel, profile, MealType.Dinner, day, picker, cancellationToken);
            }
        }

        private async Task FillSightseeingAsync(
            StandardRoute route,
            int day,
            string destination,
            PRICE_LEVEL? priceLevel,
            PlacePicker picker,
            List<string> touristicGoogleIds,
            string touristicQuery,
            List<string> afterDinnerGoogleIds,
            string afterDinnerQuery,
            CancellationToken cancellationToken)
        {
            for (int attempt = 0; ; attempt++)
            {
                try
                {
                    var touristicPlaces = await _placeSelectionService.SelectManyAsync<Place>(
                        touristicGoogleIds, picker, RouteBuilding.TouristicPlacesPerDay,
                        offset: day * RouteBuilding.TouristicPlacesPerDay,
                        cancellationToken: cancellationToken);

                    foreach (var touristicPlace in touristicPlaces)
                    {
                        touristicPlace.UserPersonalizationPref = touristicQuery;
                    }

                    route.TravelDays[day].FirstPlace = touristicPlaces[0];
                    route.TravelDays[day].SecondPlace = touristicPlaces[1];
                    route.TravelDays[day].ThirdPlace = touristicPlaces[2];

                    var afterDinnerPlace = await _placeSelectionService.SelectAsync<PlaceAfterDinner>(
                        afterDinnerGoogleIds, picker, priceLevel, offset: day, cancellationToken: cancellationToken);

                    afterDinnerPlace.UserFoodPreference = afterDinnerQuery;

                    route.TravelDays[day].PlaceAfterDinner = afterDinnerPlace;

                    return;
                }
                catch (InvalidOperationException ex)
                {
                    if (attempt >= MaxDayRetries)
                    {
                        throw new RouteGenerationException(
                            $"Day {day + 1} in {destination} could not be filled with unique places.", ex);
                    }

                    _logger.LogWarning(
                        ex,
                        "Could not select places for day {Day}; widening the search ({Attempt}/{Max}).",
                        day + 1, attempt + 1, MaxDayRetries);

                    await SearchHiddenGemsAsync(destination, touristicGoogleIds, afterDinnerGoogleIds, picker, priceLevel);
                }
            }
        }

        private async Task<TravelAccomodation> FindHotelAsync(
            string destination,
            PRICE_LEVEL? priceLevel,
            PreferenceProfile profile,
            IReadOnlyCollection<string> excludedGoogleIds)
        {
            var accommodationQuery = _placeQueryBuilder.Accommodation(profile.Accommodation, priceLevel);

            var hotels = (await _placeSearchService.SearchFirstMatchAsync(new[]
            {
                $"{accommodationQuery} in {destination}",
                $"{_placeQueryBuilder.HotelStars(priceLevel)} star hotel in {destination}",
                $"hotel in {destination}"
            })).Where(hotel => hotel.Id is not null && !excludedGoogleIds.Contains(hotel.Id)).ToList();

            if (hotels.Count == 0)
            {
                throw new RouteGenerationException($"No suitable hotel could be found in {destination}.");
            }

            var hotel = await _placeSelectionService.MaterializeAsync<TravelAccomodation>(
                hotels[_random.Next(hotels.Count)].Id!, priceLevel);

            hotel.UserAccomodationPreference = DescribeAccommodationPreference(profile.Accommodation);

            return hotel;
        }

        private async Task<T> SelectMealAsync<T>(
            string destination,
            PRICE_LEVEL? priceLevel,
            PreferenceProfile profile,
            MealType mealType,
            int dayIndex,
            PlacePicker picker,
            CancellationToken cancellationToken)
            where T : class, ISelectablePlace, IHasFoodPreference, new()
        {
            var foodQuery = _placeQueryBuilder.Food(profile.Food, priceLevel, mealType);

            var candidates = await _placeSearchService.SearchFirstMatchAsync(new[]
            {
                $"{foodQuery} in {destination}",
                $"{_placeQueryBuilder.PricePrefix(priceLevel)}{MealQuery(mealType)} in {destination}",
                $"restaurant in {destination}"
            }, cancellationToken);

            if (candidates.Count == 0)
            {
                throw new RouteGenerationException($"No {mealType} place could be found in {destination}.");
            }

            var unused = Unused(candidates, picker);

            if (unused.Count == 0)
            {
                var alternativeQuery = _placeQueryBuilder.AlternativeFood(mealType, priceLevel);
                var alternatives = await _placeSearchService.SearchAsync(
                    $"{alternativeQuery} in {destination}", cancellationToken);

                unused = Unused(alternatives, picker);
            }

            if (unused.Count == 0)
            {
                // Nothing fresh is left; reusing a place the trip already visits beats failing
                // the whole route, but it is worth knowing about.
                unused = candidates.Where(place => place.Id is not null).Select(place => place.Id!).ToList();

                _logger.LogWarning("Could not find an unused {MealType} place in {Destination}; reusing one.",
                    mealType, destination);
            }

            if (unused.Count == 0)
            {
                throw new RouteGenerationException(
                    $"No {mealType} place in {destination} carried a usable place id.");
            }

            var index = (_random.Next(unused.Count) + dayIndex) % unused.Count;
            var selectedGoogleId = unused[index];

            var meal = await _placeSelectionService.MaterializeAsync<T>(selectedGoogleId, priceLevel, cancellationToken);
            meal.UserFoodPreference = foodQuery;

            picker.MarkUsed(selectedGoogleId);

            return meal;
        }

        private async Task<List<string>> BuildTouristicPoolAsync(
            string destination,
            string touristicQuery,
            PreferenceProfile profile,
            int dayCount,
            PlacePicker picker)
        {
            var requiredCount = dayCount * TouristicPoolPerDay;

            var queries = new List<string> { $"{touristicQuery} in {destination}" };
            queries.AddRange(RouteSearchQueries.Touristic.In(destination));

            var pool = (await _placeSearchService.CollectPlaceIdsAsync(queries, requiredCount)).ToList();

            if (pool.Count < requiredCount)
            {
                var alternativeQuery = _placeQueryBuilder.AlternativeTouristic(profile.Personalization, touristicQuery);

                var widening = new List<string> { $"{alternativeQuery} in {destination}" };
                widening.AddRange(RouteSearchQueries.TouristicAlternatives.In(destination));

                pool.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    widening, Math.Max(0, requiredCount - pool.Count), pool));
            }

            return Unused(pool, picker);
        }

        private async Task<List<string>> BuildAfterDinnerPoolAsync(
            string destination,
            string afterDinnerQuery,
            PlacePicker picker)
        {
            var queries = new List<string> { $"{afterDinnerQuery} in {destination}" };
            queries.AddRange(RouteSearchQueries.AfterDinner.In(destination));

            var pool = await _placeSearchService.CollectPlaceIdsAsync(queries);

            return Unused(pool, picker);
        }

        private async Task WidenPoolsAsync(
            string destination,
            int dayCount,
            List<string> touristicGoogleIds,
            List<string> afterDinnerGoogleIds,
            PlacePicker picker)
        {
            if (touristicGoogleIds.Count < dayCount * RouteBuilding.TouristicPlacesPerDay)
            {
                touristicGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    RouteSearchQueries.TouristicLastResort.In(destination),
                    dayCount * (TouristicPoolPerDay - 1) - touristicGoogleIds.Count,
                    touristicGoogleIds.Concat(picker.UsedGoogleIds)));
            }

            if (afterDinnerGoogleIds.Count < dayCount)
            {
                afterDinnerGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    RouteSearchQueries.AfterDinnerLastResort.In(destination),
                    dayCount * AfterDinnerPoolPerDay - afterDinnerGoogleIds.Count,
                    afterDinnerGoogleIds.Concat(picker.UsedGoogleIds)));
            }
        }

        private async Task SearchHiddenGemsAsync(
            string destination,
            List<string> touristicGoogleIds,
            List<string> afterDinnerGoogleIds,
            PlacePicker picker,
            PRICE_LEVEL? priceLevel)
        {
            touristicGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                RouteSearchQueries.TouristicHiddenGems.In(destination),
                excludedPlaceIds: touristicGoogleIds.Concat(picker.UsedGoogleIds)));

            var afterDinnerQuery = _placeQueryBuilder.AlternativeAfterDinner(priceLevel);

            afterDinnerGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                new[] { $"{afterDinnerQuery} in {destination}" },
                excludedPlaceIds: afterDinnerGoogleIds.Concat(picker.UsedGoogleIds)));
        }

        /// <summary>
        /// Turns the preference the query was built around into the human readable label
        /// stored on the hotel, so the user can see why it was picked.
        /// </summary>
        private string DescribeAccommodationPreference(IReadOnlyList<PreferenceItem>? preferences)
        {
            if (preferences is null || preferences.Count == 0)
            {
                return "No specific preference";
            }

            var selected = _placeQueryBuilder.SelectPreference(preferences);

            foreach (AccommodationPreferenceTypes type in Enum.GetValues<AccommodationPreferenceTypes>())
            {
                if ($"{type}Preference" == selected)
                {
                    return type.GetDescription();
                }
            }

            return selected;
        }

        private static string MealQuery(MealType mealType) => mealType switch
        {
            MealType.Breakfast => RouteSearchQueries.Breakfast,
            MealType.Lunch => RouteSearchQueries.Lunch,
            MealType.Dinner => RouteSearchQueries.Dinner,
            _ => "restaurant "
        };

        private static List<string> Unused(IEnumerable<Application.DTOs.GooglePlaces.Place> places, PlacePicker picker) =>
            places
                .Where(place => place?.Id is not null && !picker.IsUsed(place.Id))
                .Select(place => place.Id!)
                .Distinct()
                .ToList();

        private static List<string> Unused(IEnumerable<string> googleIds, PlacePicker picker) =>
            googleIds
                .Where(googleId => !string.IsNullOrWhiteSpace(googleId) && !picker.IsUsed(googleId))
                .Distinct()
                .ToList();
    }
}
