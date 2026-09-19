using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.Extensions.Logging;
using GooglePlace = EEaseWebAPI.Application.DTOs.GooglePlaces.Place;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class RandomRouteBuilder : IRandomRouteBuilder
    {
        private const int MaxRouteAttempts = 3;
        private const int AfterDinnerPoolPerDay = 2;

        private readonly IPlaceSearchService _placeSearchService;
        private readonly IPlaceSelectionService _placeSelectionService;
        private readonly IPlaceQueryBuilder _placeQueryBuilder;
        private readonly IRouteEnrichmentService _routeEnrichmentService;
        private readonly IRoutePlanValidator _routePlanValidator;
        private readonly ISystemUserProvider _systemUserProvider;
        private readonly ILogger<RandomRouteBuilder> _logger;
        private readonly Random _random;

        public RandomRouteBuilder(
            IPlaceSearchService placeSearchService,
            IPlaceSelectionService placeSelectionService,
            IPlaceQueryBuilder placeQueryBuilder,
            IRouteEnrichmentService routeEnrichmentService,
            IRoutePlanValidator routePlanValidator,
            ISystemUserProvider systemUserProvider,
            ILogger<RandomRouteBuilder> logger,
            Random random)
        {
            _placeSearchService = placeSearchService;
            _placeSelectionService = placeSelectionService;
            _placeQueryBuilder = placeQueryBuilder;
            _routeEnrichmentService = routeEnrichmentService;
            _routePlanValidator = routePlanValidator;
            _systemUserProvider = systemUserProvider;
            _logger = logger;
            _random = random;
        }

        public async Task<StandardRoute> BuildAsync(
            string destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            CancellationToken cancellationToken = default)
        {
            RouteBuilding.ValidateParameters(destination, startDate, endDate);

            destination = RouteBuilding.FormatDestination(destination);

            var dayCount = RouteBuilding.CalculateDayCount(startDate, endDate);
            var touristicNeeded = dayCount * RouteBuilding.TouristicPlacesPerDay;

            var owner = await _systemUserProvider.GetOrCreateAsync();

            var accomodation = await FindHotelAsync(destination, priceLevel);

            var pricePrefix = _placeQueryBuilder.PricePrefix(priceLevel);
            var (breakfast, lunch, dinner) = await FindFoodPlacesAsync(destination, pricePrefix, dayCount, priceLevel);

            var touristicGoogleIds = (await FindTouristicPlacesAsync(destination, touristicNeeded)).ToList();

            if (touristicGoogleIds.Count < touristicNeeded)
            {
                touristicGoogleIds.AddRange(
                    await FindMoreTouristicPlacesAsync(destination, touristicNeeded, touristicGoogleIds));
            }

            var afterDinnerGoogleIds = (await FindAfterDinnerPlacesAsync(destination, pricePrefix, dayCount)).ToList();

            var breakfastGoogleIds = breakfast.Select(place => place.Id!).ToList();
            var lunchGoogleIds = lunch.Select(place => place.Id!).ToList();
            var dinnerGoogleIds = dinner.Select(place => place.Id!).ToList();

            EnsureEnoughPlaces(
                breakfast.Count, lunch.Count, dinner.Count,
                touristicGoogleIds.Count, afterDinnerGoogleIds.Count, dayCount);

            StandardRoute? standardRoute = null;
            var validation = RoutePlanValidationResult.Empty;

            for (int attempt = 1; attempt <= MaxRouteAttempts && !validation.IsValid; attempt++)
            {
                standardRoute = RouteBuilding.InitializeStandardRoute(destination, dayCount, owner.Id);

                try
                {
                    var picker = new PlacePicker(_random);

                    for (int day = 0; day < dayCount; day++)
                    {
                        standardRoute.TravelDays[day] = await BuildDayAsync(
                            accomodation, priceLevel, picker,
                            breakfastGoogleIds, lunchGoogleIds, dinnerGoogleIds,
                            afterDinnerGoogleIds, touristicGoogleIds,
                            cancellationToken);
                    }

                    validation = _routePlanValidator.Validate(standardRoute);

                    if (!validation.IsValid)
                    {
                        _logger.LogWarning(
                            "Attempt {Attempt}/{Max} produced an unusable route: {Problems}",
                            attempt, MaxRouteAttempts, validation.Describe());
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt}/{Max} could not build the route.", attempt, MaxRouteAttempts);

                    if (attempt == MaxRouteAttempts)
                    {
                        throw new RouteGenerationException($"No usable route could be built for {destination}.", ex);
                    }
                }
            }

            if (!validation.IsValid)
            {
                throw new RouteGenerationException(
                    $"No usable route could be built for {destination} after {MaxRouteAttempts} attempts ({validation.Describe()}).");
            }

            await _routeEnrichmentService.ApplyAsync(standardRoute!, startDate, endDate);

            return standardRoute!;
        }

        private async Task<TravelDay> BuildDayAsync(
            TravelAccomodation accomodation,
            PRICE_LEVEL? priceLevel,
            PlacePicker picker,
            IReadOnlyList<string> breakfastGoogleIds,
            IReadOnlyList<string> lunchGoogleIds,
            IReadOnlyList<string> dinnerGoogleIds,
            IReadOnlyList<string> afterDinnerGoogleIds,
            IReadOnlyList<string> touristicGoogleIds,
            CancellationToken cancellationToken)
        {
            var day = new TravelDay
            {
                Accomodation = RouteBuilding.CopyAccommodation(accomodation, priceLevel)
            };

            day.Breakfast = await _placeSelectionService.SelectAsync<Breakfast>(
                breakfastGoogleIds, picker, priceLevel, cancellationToken: cancellationToken);

            day.Lunch = await _placeSelectionService.SelectAsync<Lunch>(
                lunchGoogleIds, picker, priceLevel, cancellationToken: cancellationToken);

            day.Dinner = await _placeSelectionService.SelectAsync<Dinner>(
                dinnerGoogleIds, picker, priceLevel, cancellationToken: cancellationToken);

            day.PlaceAfterDinner = await _placeSelectionService.SelectAsync<PlaceAfterDinner>(
                afterDinnerGoogleIds, picker, priceLevel, cancellationToken: cancellationToken);

            var touristicPlaces = await _placeSelectionService.SelectManyAsync<Place>(
                touristicGoogleIds, picker, RouteBuilding.TouristicPlacesPerDay,
                cancellationToken: cancellationToken);

            day.FirstPlace = touristicPlaces[0];
            day.SecondPlace = touristicPlaces[1];
            day.ThirdPlace = touristicPlaces[2];

            return day;
        }

        private async Task<TravelAccomodation> FindHotelAsync(string destination, PRICE_LEVEL? priceLevel)
        {
            var stars = _placeQueryBuilder.HotelStars(priceLevel);

            var hotels = await _placeSearchService.SearchFirstMatchAsync(new[]
            {
                $"{stars} star hotel in {destination}",
                $"hotel in {destination}"
            });

            if (hotels.Count == 0)
            {
                throw new RouteGenerationException($"No hotel could be found in {destination}.");
            }

            var hotel = hotels[_random.Next(hotels.Count)];

            var accomodation = await _placeSelectionService.MaterializeAsync<TravelAccomodation>(hotel.Id!, priceLevel);
            accomodation.Star = stars;
            accomodation.UserAccomodationPreference = "random";

            return accomodation;
        }

        private async Task<(IReadOnlyList<GooglePlace> Breakfast, IReadOnlyList<GooglePlace> Lunch, IReadOnlyList<GooglePlace> Dinner)>
            FindFoodPlacesAsync(string destination, string pricePrefix, int dayCount, PRICE_LEVEL? priceLevel)
        {
            var fallbackPrefix = _placeQueryBuilder.PricePrefix(NextPriceLevelDown(priceLevel));

            var breakfast = await FindMealsAsync(destination, pricePrefix, fallbackPrefix, RouteSearchQueries.Breakfast, dayCount);
            var lunch = await FindMealsAsync(destination, pricePrefix, fallbackPrefix, RouteSearchQueries.Lunch, dayCount);
            var dinner = await FindMealsAsync(destination, pricePrefix, fallbackPrefix, RouteSearchQueries.Dinner, dayCount);

            return (breakfast, lunch, dinner);
        }

        private async Task<IReadOnlyList<GooglePlace>> FindMealsAsync(
            string destination,
            string pricePrefix,
            string fallbackPrefix,
            string mealQuery,
            int dayCount)
        {
            var places = await _placeSearchService.SearchAsync($"{pricePrefix} {mealQuery} in {destination}");

            if (places.Count >= dayCount)
            {
                return places;
            }

            var fallback = await _placeSearchService.SearchAsync($"{fallbackPrefix}{mealQuery} in {destination}");

            return places
                .Concat(fallback)
                .GroupBy(place => place.Id)
                .Select(group => group.First())
                .ToList();
        }

        private Task<IReadOnlyList<string>> FindTouristicPlacesAsync(string destination, int requiredCount) =>
            _placeSearchService.CollectPlaceIdsAsync(
                RouteSearchQueries.Touristic.Concat(RouteSearchQueries.TouristicWidening).In(destination),
                requiredCount);

        private Task<IReadOnlyList<string>> FindMoreTouristicPlacesAsync(
            string destination,
            int requiredCount,
            IReadOnlyCollection<string> existingIds) =>
            _placeSearchService.CollectPlaceIdsAsync(
                RouteSearchQueries.TouristicAlternatives.In(destination),
                Math.Max(0, requiredCount - existingIds.Count),
                existingIds);

        private Task<IReadOnlyList<string>> FindAfterDinnerPlacesAsync(string destination, string pricePrefix, int dayCount) =>
            _placeSearchService.CollectPlaceIdsAsync(
                RouteSearchQueries.AfterDinner.Select(query => $"{pricePrefix} {query.TrimEnd()} in {destination}"),
                dayCount * AfterDinnerPoolPerDay);

        private static void EnsureEnoughPlaces(
            int breakfastCount, int lunchCount, int dinnerCount,
            int touristicCount, int afterDinnerCount, int dayCount)
        {
            var missing = new List<string>();

            if (breakfastCount < dayCount) missing.Add("breakfast");
            if (lunchCount < dayCount) missing.Add("lunch");
            if (dinnerCount < dayCount) missing.Add("dinner");
            if (afterDinnerCount < dayCount) missing.Add("after dinner");
            if (touristicCount < dayCount * RouteBuilding.TouristicPlacesPerDay) missing.Add("touristic");

            if (missing.Count > 0)
            {
                throw new RouteGenerationException(
                    $"Not enough places for {dayCount} day(s) even after widening the search: {string.Join(", ", missing)}.");
            }
        }

        /// <summary>
        /// When a price level yields too few results the search is repeated one step cheaper.
        /// The cheapest level has nowhere lower to go, so it widens upwards instead.
        /// </summary>
        private static PRICE_LEVEL NextPriceLevelDown(PRICE_LEVEL? currentLevel) =>
            currentLevel switch
            {
                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE,
                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => PRICE_LEVEL.PRICE_LEVEL_MODERATE,
                PRICE_LEVEL.PRICE_LEVEL_MODERATE => PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE,
                _ => PRICE_LEVEL.PRICE_LEVEL_MODERATE
            };
    }
}
