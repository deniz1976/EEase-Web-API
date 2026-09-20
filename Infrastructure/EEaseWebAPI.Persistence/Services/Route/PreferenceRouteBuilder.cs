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

            // The picker starts out holding every disliked place, so they are never handed back.
            var picker = new PlacePicker(_random, dislikedGoogleIds);

            var touristicQuery = _placeQueryBuilder.Touristic(profile.Personalization);
            var afterDinnerQuery = _placeQueryBuilder.AfterDinner(profile.Personalization, priceLevel);

            // The hotel, the sights and the evening venues are three searches that know
            // nothing of each other, and the meals are searched for while they are still in
            // the air. The pools only read what the picker has already handed out, so a pool
            // that was collected a moment earlier costs nothing: a place that has since been
            // taken is skipped when it is picked, not when it is collected.
            var hotelSearch = FindHotelAsync(
                city, priceLevel, profile, dislikedGoogleIds, cancellationToken);
            var touristicSearch = BuildTouristicPoolAsync(
                city, touristicQuery, profile, dayCount, picker, cancellationToken);
            var afterDinnerSearch = BuildAfterDinnerPoolAsync(
                city, afterDinnerQuery, picker, cancellationToken);

            var accommodation = await hotelSearch;

            foreach (var travelDay in standardRoute.TravelDays)
            {
                travelDay.Accomodation = RouteBuilding.CopyAccommodation(accommodation, priceLevel);
            }

            await FillMealsAsync(standardRoute, city, priceLevel, profile, picker, dayCount, cancellationToken);

            var touristicGoogleIds = await touristicSearch;
            var afterDinnerGoogleIds = await afterDinnerSearch;

            if (touristicGoogleIds.Count < dayCount * RouteBuilding.TouristicPlacesPerDay ||
                afterDinnerGoogleIds.Count < dayCount)
            {
                await WidenPoolsAsync(
                    city, dayCount, touristicGoogleIds, afterDinnerGoogleIds, picker, cancellationToken);
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
            // Three searches, not three per day. The query behind a breakfast on day one is
            // the query behind a breakfast on day five, so the trip used to ask Google the
            // same question once for every day it lasted. The three do not wait for each
            // other either.
            var breakfastSearch = SearchMealAsync(
                destination, priceLevel, profile, MealType.Breakfast, cancellationToken);
            var lunchSearch = SearchMealAsync(
                destination, priceLevel, profile, MealType.Lunch, cancellationToken);
            var dinnerSearch = SearchMealAsync(
                destination, priceLevel, profile, MealType.Dinner, cancellationToken);

            await Task.WhenAll(breakfastSearch, lunchSearch, dinnerSearch);

            var breakfasts = await breakfastSearch;
            var lunches = await lunchSearch;
            var dinners = await dinnerSearch;

            var breakfastIds = new string[dayCount];
            var lunchIds = new string[dayCount];
            var dinnerIds = new string[dayCount];

            // Claiming is ordered, so no two days sit down at the same table.
            for (int day = 0; day < dayCount; day++)
            {
                breakfastIds[day] = await ClaimMealAsync(
                    breakfasts, destination, priceLevel, day, picker, cancellationToken);
                lunchIds[day] = await ClaimMealAsync(
                    lunches, destination, priceLevel, day, picker, cancellationToken);
                dinnerIds[day] = await ClaimMealAsync(
                    dinners, destination, priceLevel, day, picker, cancellationToken);
            }

            // Reading the details of what was claimed is not.
            var breakfastPlaces = MaterializeMealsAsync<Breakfast>(
                breakfastIds, breakfasts.FoodQuery, priceLevel, cancellationToken);
            var lunchPlaces = MaterializeMealsAsync<Lunch>(
                lunchIds, lunches.FoodQuery, priceLevel, cancellationToken);
            var dinnerPlaces = MaterializeMealsAsync<Dinner>(
                dinnerIds, dinners.FoodQuery, priceLevel, cancellationToken);

            await Task.WhenAll(breakfastPlaces, lunchPlaces, dinnerPlaces);

            for (int day = 0; day < dayCount; day++)
            {
                route.TravelDays[day].Breakfast = (await breakfastPlaces)[day];
                route.TravelDays[day].Lunch = (await lunchPlaces)[day];
                route.TravelDays[day].Dinner = (await dinnerPlaces)[day];
            }
        }

        private async Task<T[]> MaterializeMealsAsync<T>(
            IReadOnlyList<string> googleIds,
            string foodQuery,
            PRICE_LEVEL? priceLevel,
            CancellationToken cancellationToken)
            where T : class, ISelectablePlace, IHasFoodPreference, new()
        {
            var meals = await Task.WhenAll(googleIds.Select(googleId =>
                _placeSelectionService.MaterializeAsync<T>(googleId, priceLevel, cancellationToken)));

            foreach (var meal in meals)
            {
                meal.UserFoodPreference = foodQuery;
            }

            return meals;
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

                    await SearchHiddenGemsAsync(
                        destination, touristicGoogleIds, afterDinnerGoogleIds, picker, priceLevel, cancellationToken);
                }
            }
        }

        private async Task<TravelAccomodation> FindHotelAsync(
            string destination,
            PRICE_LEVEL? priceLevel,
            PreferenceProfile profile,
            IReadOnlyCollection<string> excludedGoogleIds,
            CancellationToken cancellationToken)
        {
            var accommodationQuery = _placeQueryBuilder.Accommodation(profile.Accommodation, priceLevel);

            var hotels = (await _placeSearchService.SearchFirstMatchAsync(
                new[]
                {
                    $"{accommodationQuery} in {destination}",
                    $"{_placeQueryBuilder.HotelStars(priceLevel)} star hotel in {destination}",
                    $"hotel in {destination}"
                },
                cancellationToken)).Where(hotel => hotel.Id is not null && !excludedGoogleIds.Contains(hotel.Id)).ToList();

            if (hotels.Count == 0)
            {
                throw new RouteGenerationException($"No suitable hotel could be found in {destination}.");
            }

            var hotel = await _placeSelectionService.MaterializeAsync<TravelAccomodation>(
                hotels[_random.Next(hotels.Count)].Id!, priceLevel, cancellationToken);

            hotel.UserAccomodationPreference = DescribeAccommodationPreference(profile.Accommodation);

            return hotel;
        }

        private async Task<MealCandidates> SearchMealAsync(
            string destination,
            PRICE_LEVEL? priceLevel,
            PreferenceProfile profile,
            MealType mealType,
            CancellationToken cancellationToken)
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

            return new MealCandidates(mealType, foodQuery, candidates);
        }

        /// <summary>
        /// Takes one place out of what the meal was searched for and marks it as spoken for,
        /// so the rest of the trip cannot sit down at the same table.
        /// </summary>
        private async Task<string> ClaimMealAsync(
            MealCandidates candidates,
            string destination,
            PRICE_LEVEL? priceLevel,
            int dayIndex,
            PlacePicker picker,
            CancellationToken cancellationToken)
        {
            var unused = Unused(candidates.Places, picker);

            if (unused.Count == 0)
            {
                // Every candidate is already on the trip. A different phrasing turns up places
                // the first search did not, and one phrasing is enough for the whole trip.
                candidates.Alternatives ??= await _placeSearchService.SearchAsync(
                    $"{_placeQueryBuilder.AlternativeFood(candidates.MealType, priceLevel)} in {destination}",
                    cancellationToken);

                unused = Unused(candidates.Alternatives, picker);
            }

            if (unused.Count == 0)
            {
                // Nothing fresh is left; reusing a place the trip already visits beats failing
                // the whole route, but it is worth knowing about.
                unused = candidates.Places
                    .Where(place => place.Id is not null)
                    .Select(place => place.Id!)
                    .ToList();

                _logger.LogWarning("Could not find an unused {MealType} place in {Destination}; reusing one.",
                    candidates.MealType, destination);
            }

            if (unused.Count == 0)
            {
                throw new RouteGenerationException(
                    $"No {candidates.MealType} place in {destination} carried a usable place id.");
            }

            var selectedGoogleId = unused[(_random.Next(unused.Count) + dayIndex) % unused.Count];

            picker.MarkUsed(selectedGoogleId);

            return selectedGoogleId;
        }

        /// <summary>
        /// What a meal may be chosen from. The search behind it depends on the meal and the
        /// city, never on the day, so every day of the trip picks out of the same list.
        /// </summary>
        private sealed class MealCandidates
        {
            public MealCandidates(
                MealType mealType,
                string foodQuery,
                IReadOnlyList<Application.DTOs.GooglePlaces.Place> places)
            {
                MealType = mealType;
                FoodQuery = foodQuery;
                Places = places;
            }

            public MealType MealType { get; }

            public string FoodQuery { get; }

            public IReadOnlyList<Application.DTOs.GooglePlaces.Place> Places { get; }

            /// <summary>
            /// Searched for the first time a day finds every candidate already taken, and
            /// kept for the days after it.
            /// </summary>
            public IReadOnlyList<Application.DTOs.GooglePlaces.Place>? Alternatives { get; set; }
        }

        private async Task<List<string>> BuildTouristicPoolAsync(
            string destination,
            string touristicQuery,
            PreferenceProfile profile,
            int dayCount,
            PlacePicker picker,
            CancellationToken cancellationToken)
        {
            var requiredCount = dayCount * TouristicPoolPerDay;

            var queries = new List<string> { $"{touristicQuery} in {destination}" };
            queries.AddRange(RouteSearchQueries.Touristic.In(destination));

            var pool = (await _placeSearchService.CollectPlaceIdsAsync(
                queries, requiredCount, cancellationToken: cancellationToken)).ToList();

            if (pool.Count < requiredCount)
            {
                var alternativeQuery = _placeQueryBuilder.AlternativeTouristic(profile.Personalization, touristicQuery);

                var widening = new List<string> { $"{alternativeQuery} in {destination}" };
                widening.AddRange(RouteSearchQueries.TouristicAlternatives.In(destination));

                pool.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    widening, Math.Max(0, requiredCount - pool.Count), pool, cancellationToken));
            }

            return Unused(pool, picker);
        }

        private async Task<List<string>> BuildAfterDinnerPoolAsync(
            string destination,
            string afterDinnerQuery,
            PlacePicker picker,
            CancellationToken cancellationToken)
        {
            var queries = new List<string> { $"{afterDinnerQuery} in {destination}" };
            queries.AddRange(RouteSearchQueries.AfterDinner.In(destination));

            var pool = await _placeSearchService.CollectPlaceIdsAsync(
                queries, cancellationToken: cancellationToken);

            return Unused(pool, picker);
        }

        private async Task WidenPoolsAsync(
            string destination,
            int dayCount,
            List<string> touristicGoogleIds,
            List<string> afterDinnerGoogleIds,
            PlacePicker picker,
            CancellationToken cancellationToken)
        {
            if (touristicGoogleIds.Count < dayCount * RouteBuilding.TouristicPlacesPerDay)
            {
                touristicGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    RouteSearchQueries.TouristicLastResort.In(destination),
                    dayCount * (TouristicPoolPerDay - 1) - touristicGoogleIds.Count,
                    touristicGoogleIds.Concat(picker.UsedGoogleIds),
                    cancellationToken));
            }

            if (afterDinnerGoogleIds.Count < dayCount)
            {
                afterDinnerGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    RouteSearchQueries.AfterDinnerLastResort.In(destination),
                    dayCount * AfterDinnerPoolPerDay - afterDinnerGoogleIds.Count,
                    afterDinnerGoogleIds.Concat(picker.UsedGoogleIds),
                    cancellationToken));
            }
        }

        private async Task SearchHiddenGemsAsync(
            string destination,
            List<string> touristicGoogleIds,
            List<string> afterDinnerGoogleIds,
            PlacePicker picker,
            PRICE_LEVEL? priceLevel,
            CancellationToken cancellationToken)
        {
            touristicGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                RouteSearchQueries.TouristicHiddenGems.In(destination),
                excludedPlaceIds: touristicGoogleIds.Concat(picker.UsedGoogleIds),
                cancellationToken: cancellationToken));

            var afterDinnerQuery = _placeQueryBuilder.AlternativeAfterDinner(priceLevel);

            afterDinnerGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                new[] { $"{afterDinnerQuery} in {destination}" },
                excludedPlaceIds: afterDinnerGoogleIds.Concat(picker.UsedGoogleIds),
                cancellationToken: cancellationToken));
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
