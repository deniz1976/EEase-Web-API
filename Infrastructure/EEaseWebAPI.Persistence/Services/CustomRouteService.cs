using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.MapEntities.GeminiAI;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Domain.Extensions;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

using System.Text;

using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Services
{
    public class CustomRouteService : ICustomRouteService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IPlaceSearchService _placeSearchService;
        private readonly IPlaceSelectionService _placeSelectionService;
        private readonly IPlaceQueryBuilder _placeQueryBuilder;
        private readonly IPreferenceProfileBuilder _preferenceProfileBuilder;
        private readonly EEaseAPIDbContext _context;
        private readonly ISystemUserProvider _systemUserProvider;
        private readonly IRouteEnrichmentService _routeEnrichmentService;
        private readonly IRoutePlanValidator _routePlanValidator;
        private readonly IFriendshipService _friendshipService;
        private readonly IDislikedPlaceService _dislikedPlaceService;
        private readonly ILogger<CustomRouteService> _logger;
        private readonly Random _random;

        private const int MaxRouteAttempts = 3;
        private const int MaxDayRetries = 3;
        private const int PublishedRouteStatus = 2;
        private const int TouristicPlacesPerDay = 3;


        private readonly static string BREAKFAST_QUERY = "Breakfast restaurant ";
        private readonly static string LUNCH_QUERY = "Lunch restaurant ";
        private readonly static string DINNER_QUERY = "Dinner restaurant ";

        private readonly static string AFTER_DINNER_QUERY1 = "Live music bars ";
        private readonly static string AFTER_DINNER_QUERY2 = "Modern rooftop bars ";
        private readonly static string AFTER_DINNER_QUERY3 = "Famous cocktail bars ";
        private readonly static string AFTER_DINNER_QUERY4 = "Special dessert shops ";
        private readonly static string AFTER_DINNER_QUERY5 = "Trendy nightclubs ";

        private readonly static string EXPENSIVE_QUERY = "Expensive ";
        private readonly static string MODERATE_QUERY = "Moderate ";
        private readonly static string INEXPENSIVE_QUERY = "Inexpensive ";

        private readonly static string TOURISTIC_QUERY = "Touristic places ";
        private readonly static string TOURISTIC_QUERY1 = "Historic landmarks and museums ";
        private readonly static string TOURISTIC_QUERY2 = "Hidden gem sightseeing spots ";
        private readonly static string TOURISTIC_QUERY3 = "Breathtaking natural attractions ";
        private readonly static string TOURISTIC_QUERY4 = "Off the beaten path tourist spots ";

        public CustomRouteService(
            UserManager<AppUser> userManager,
            IPlaceSearchService placeSearchService,
            IPlaceSelectionService placeSelectionService,
            IPlaceQueryBuilder placeQueryBuilder,
            IPreferenceProfileBuilder preferenceProfileBuilder,
            EEaseAPIDbContext context,
            ISystemUserProvider systemUserProvider,
            IRouteEnrichmentService routeEnrichmentService,
            IRoutePlanValidator routePlanValidator,
            IFriendshipService friendshipService,
            IDislikedPlaceService dislikedPlaceService,
            ILogger<CustomRouteService> logger,
            Random random)
        {
            _userManager = userManager;
            _placeSearchService = placeSearchService;
            _placeSelectionService = placeSelectionService;
            _placeQueryBuilder = placeQueryBuilder;
            _preferenceProfileBuilder = preferenceProfileBuilder;
            _context = context;
            _systemUserProvider = systemUserProvider;
            _routeEnrichmentService = routeEnrichmentService;
            _routePlanValidator = routePlanValidator;
            _friendshipService = friendshipService;
            _dislikedPlaceService = dislikedPlaceService;
            _logger = logger;
            _random = random;
        }

        public async Task<StandardRoute> CreateRandomRoute(string destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL)
        {
            ValidateParameters(destination, startDate, endDate);

            destination = FormatDestination(destination);

            var dayCount = CalculateDayCount(startDate, endDate);

            var admin = await GetAdminUser();

            var accomodation = await FindAndSelectHotel(destination, _PRICE_LEVEL);

            string priceQueryPrefix = _placeQueryBuilder.PricePrefix(_PRICE_LEVEL);
            var (breakfastPlaces, lunchPlaces, dinnerPlaces) = await FindFoodPlaces(destination, priceQueryPrefix, dayCount, _PRICE_LEVEL);

            var touristicGoogleIds = (await FindTouristicPlaces(destination, dayCount * 3)).ToList();

            if (touristicGoogleIds.Count < (dayCount * 3))
            {
                var additionalPlaces = await FindMoreTouristicPlaces(destination, dayCount * 3, touristicGoogleIds);
                touristicGoogleIds.AddRange(additionalPlaces);
            }

            var afterDinnerGoogleIds = (await FindAfterDinnerPlaces(destination, priceQueryPrefix, dayCount, _PRICE_LEVEL)).ToList();

            var breakfastGoogleIds = breakfastPlaces.Select(place => place.Id!).ToList();
            var lunchGoogleIds = lunchPlaces.Select(place => place.Id!).ToList();
            var dinnerGoogleIds = dinnerPlaces.Select(place => place.Id!).ToList();

            EnsureEnoughPlaces(
                breakfastPlaces.Count, lunchPlaces.Count, dinnerPlaces.Count,
                touristicGoogleIds.Count, afterDinnerGoogleIds.Count, dayCount);

            StandardRoute standardRoute = null;
            RoutePlanValidationResult validation = RoutePlanValidationResult.Empty;

            for (int attempt = 1; attempt <= MaxRouteAttempts && !validation.IsValid; attempt++)
            {
                standardRoute = InitializeStandardRoute(destination, dayCount, admin.Id);

                try
                {
                    var picker = new PlacePicker(_random);

                    for (int i = 0; i < dayCount; i++)
                    {
                        var day = new TravelDay();

                        day.Accomodation = CreateAccommodation(accomodation, _PRICE_LEVEL);

                        day.Breakfast = await _placeSelectionService.SelectAsync<Breakfast>(
                            breakfastGoogleIds, picker, _PRICE_LEVEL);

                        day.Lunch = await _placeSelectionService.SelectAsync<Lunch>(
                            lunchGoogleIds, picker, _PRICE_LEVEL);

                        day.Dinner = await _placeSelectionService.SelectAsync<Dinner>(
                            dinnerGoogleIds, picker, _PRICE_LEVEL);

                        day.PlaceAfterDinner = await _placeSelectionService.SelectAsync<PlaceAfterDinner>(
                            afterDinnerGoogleIds, picker, _PRICE_LEVEL);

                        var touristicPlaces = await _placeSelectionService.SelectManyAsync<Domain.Entities.Route.Place>(
                            touristicGoogleIds, picker, 3);

                        day.FirstPlace = touristicPlaces[0];
                        day.SecondPlace = touristicPlaces[1];
                        day.ThirdPlace = touristicPlaces[2];

                        standardRoute.TravelDays[i] = day;
                    }

                    validation = _routePlanValidator.Validate(standardRoute);

                    if (!validation.IsValid)
                    {
                        _logger.LogWarning(
                            "Attempt {Attempt}/{Max} produced an unusable route: {Problems}",
                            attempt,
                            MaxRouteAttempts,
                            validation.Describe());
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Attempt {Attempt}/{Max} could not build the route.", attempt, MaxRouteAttempts);

                    if (attempt == MaxRouteAttempts)
                    {
                        throw new RouteGenerationException(
                            $"No usable route could be built for {destination}.", ex);
                    }
                }
            }

            if (!validation.IsValid)
            {
                throw new RouteGenerationException(
                    $"No usable route could be built for {destination} after {MaxRouteAttempts} attempts ({validation.Describe()}).");
            }

            await _routeEnrichmentService.ApplyAsync(standardRoute, startDate, endDate);

            await SaveRouteToDatabase(standardRoute);
            standardRoute.User = null;

            return standardRoute;
        }

        private static void ValidateParameters(string? destination, DateOnly? startDate, DateOnly? endDate)
        {
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentNullException(nameof(destination));

            if (startDate == null)
                throw new ArgumentNullException(nameof(startDate));

            if (endDate == null)
                throw new ArgumentNullException(nameof(endDate));

            if (endDate < startDate)
                throw new ArgumentException("The end date cannot be before the start date.", nameof(endDate));
        }

        private static string FormatDestination(string destination)
        {
            destination = destination.Trim();

            return char.ToUpperInvariant(destination[0]) + destination[1..].ToLowerInvariant();
        }

        private static int CalculateDayCount(DateOnly? startDate, DateOnly? endDate) =>
            (endDate!.Value.DayNumber - startDate!.Value.DayNumber) + 1;

        private Task<AppUser> GetAdminUser() => _systemUserProvider.GetOrCreateAsync();

        private StandardRoute InitializeStandardRoute(string destination, int dayCount, string adminId)
        {
            var standardRoute = new StandardRoute()
            {
                City = destination,
                User = null,
                LikedUsers = new List<AppUser>(),
                Name = destination,
                UserId = adminId,
                Days = dayCount,
                Id = Guid.NewGuid(),
                LikeCount = 0,
                TravelDays = new List<TravelDay>(),
                Status = PublishedRouteStatus
            };

            for (int i = 0; i < dayCount; i++)
            {
                standardRoute.TravelDays.Add(new TravelDay());
            }

            return standardRoute;
        }

        private async Task<TravelAccomodation> FindAndSelectHotel(string destination, PRICE_LEVEL? priceLevel)
        {
            string queryPrefix = _placeQueryBuilder.HotelStars(priceLevel);

            var hotels = await _placeSearchService.SearchFirstMatchAsync(new[]
            {
                $"{queryPrefix} star hotel in {destination}",
                $"hotel in {destination}"
            });

            if (hotels.Count == 0)
            {
                throw new RouteGenerationException($"No hotel could be found in {destination}.");
            }

            var hotel = hotels[_random.Next(hotels.Count)];

            var accomodation = await _placeSelectionService.MaterializeAsync<TravelAccomodation>(hotel.Id!, priceLevel);
            accomodation.Star = queryPrefix;
            accomodation.UserAccomodationPreference = "random";

            return accomodation;
        }

        private async Task<(IReadOnlyList<Application.DTOs.GooglePlaces.Place> Breakfast,
            IReadOnlyList<Application.DTOs.GooglePlaces.Place> Lunch,
            IReadOnlyList<Application.DTOs.GooglePlaces.Place> Dinner)>
            FindFoodPlaces(string destination, string priceQueryPrefix, int dayCount, PRICE_LEVEL? priceLevel)
        {
            var fallbackPrefix = GetNextPriceLevel(priceLevel);

            var breakfast = await FindMeals(destination, priceQueryPrefix, fallbackPrefix, BREAKFAST_QUERY, dayCount);
            var lunch = await FindMeals(destination, priceQueryPrefix, fallbackPrefix, LUNCH_QUERY, dayCount);
            var dinner = await FindMeals(destination, priceQueryPrefix, fallbackPrefix, DINNER_QUERY, dayCount);

            return (breakfast, lunch, dinner);
        }

        private async Task<IReadOnlyList<Application.DTOs.GooglePlaces.Place>> FindMeals(
            string destination,
            string priceQueryPrefix,
            string fallbackPrefix,
            string mealQuery,
            int dayCount)
        {
            var places = await _placeSearchService.SearchAsync($"{priceQueryPrefix} {mealQuery} in {destination}");

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

        private Task<IReadOnlyList<string>> FindTouristicPlaces(string destination, int requiredCount = 0)
        {
            var queries = new[]
            {
                TOURISTIC_QUERY,
                TOURISTIC_QUERY1,
                TOURISTIC_QUERY2,
                TOURISTIC_QUERY3,
                TOURISTIC_QUERY4,
                "Famous tourist attractions ",
                "Must visit spots ",
                "Top rated attractions "
            }.Select(query => $"{query}in {destination}");

            return _placeSearchService.CollectPlaceIdsAsync(queries, requiredCount);
        }

        private Task<IReadOnlyList<string>> FindMoreTouristicPlaces(
            string destination,
            int requiredCount,
            IReadOnlyCollection<string> existingIds)
        {
            var queries = new[]
            {
                "Famous tourist attractions in ",
                "Must-see places in ",
                "Popular tourist destinations in ",
                "Top-rated places to visit in ",
                "Cultural attractions in ",
                "Historical sites in "
            }.Select(query => $"{query}{destination}");

            return _placeSearchService.CollectPlaceIdsAsync(
                queries,
                Math.Max(0, requiredCount - existingIds.Count),
                existingIds);
        }

        private Task<IReadOnlyList<string>> FindAfterDinnerPlaces(
            string destination,
            string priceQueryPrefix,
            int dayCount,
            PRICE_LEVEL? priceLevel)
        {
            var queries = new[]
            {
                AFTER_DINNER_QUERY1,
                AFTER_DINNER_QUERY2,
                AFTER_DINNER_QUERY3,
                AFTER_DINNER_QUERY4,
                AFTER_DINNER_QUERY5
            }.Select(query => $"{priceQueryPrefix} {query} in {destination}");

            return _placeSearchService.CollectPlaceIdsAsync(queries, dayCount * 2);
        }

        private static void EnsureEnoughPlaces(
            int breakfastCount, int lunchCount, int dinnerCount,
            int touristicCount, int afterDinnerCount, int dayCount)
        {
            var missing = new List<string>();

            if (breakfastCount < dayCount) missing.Add("breakfast");
            if (lunchCount < dayCount) missing.Add("lunch");
            if (dinnerCount < dayCount) missing.Add("dinner");
            if (afterDinnerCount < dayCount) missing.Add("after dinner");
            if (touristicCount < dayCount * TouristicPlacesPerDay) missing.Add("touristic");

            if (missing.Count > 0)
            {
                throw new RouteGenerationException(
                    $"Not enough places for {dayCount} day(s) even after widening the search: {string.Join(", ", missing)}.");
            }
        }

        private static TravelAccomodation CreateAccommodation(TravelAccomodation baseAccommodation, PRICE_LEVEL? priceLevel)
        {
            TravelAccomodation accommodation = new TravelAccomodation()
            {
                Id = Guid.NewGuid(),
                _PRICE_LEVEL = priceLevel,
                DisplayName = baseAccommodation.DisplayName,
                FormattedAddress = baseAccommodation.FormattedAddress,
                GoogleId = baseAccommodation.GoogleId,
                GoogleMapsUri = baseAccommodation.GoogleMapsUri,
                Location = baseAccommodation.Location,
                NationalPhoneNumber = baseAccommodation.NationalPhoneNumber,
                Photos = baseAccommodation.Photos,
                PrimaryType = baseAccommodation.PrimaryType,
                Rating = baseAccommodation.Rating,
                RegularOpeningHours = baseAccommodation.RegularOpeningHours,
                Restroom = baseAccommodation.Restroom,
                WebsiteUri = baseAccommodation.WebsiteUri,
                UserAccomodationPreference = baseAccommodation.UserAccomodationPreference
            };

            return accommodation;
        }

        private async Task SaveRouteToDatabase(StandardRoute route)
        {
            await _context.StandardRoutes.AddAsync(route);
            await _context.SaveChangesAsync();
        }

        private static string GetNextPriceLevel(PRICE_LEVEL? currentLevel)
        {
            return currentLevel switch
            {
                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => EXPENSIVE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => MODERATE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_MODERATE => INEXPENSIVE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => MODERATE_QUERY,
                _ => MODERATE_QUERY
            };
        }

        public async Task<StandardRoute> CreatePrefRoute(
            string? destination,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? price_level,
            string? username,
            List<string>? friends)
        {

            ValidateParameters(destination, startDate, endDate, username);
            destination = FormatDestination(destination);
            int dayCount = CalculateDayCount(startDate, endDate);

            AppUser? user = await _userManager.FindByNameAsync(username);
            if (user == null)
            {
                throw new UserNotFoundException("User not found");
            }
            if (price_level == null)
            {
                price_level = PRICE_LEVEL.PRICE_LEVEL_MODERATE;
            }

            var allPreferences = await GatherPreferencesAsync(user, username!, friends);

            var profile = _preferenceProfileBuilder.Build(allPreferences);

            var accommodationPrefs = profile.Accommodation;
            var foodPrefs = profile.Food;
            var personalizationPrefs = profile.Personalization;

            var standardRoute = InitializeStandardRoute(destination, dayCount, user.Id);

            var dislikedGoogleIds = await _dislikedPlaceService.GetGoogleIdsAsync(user.Id);

            string accommodationQueryPrefix = _placeQueryBuilder.Accommodation(accommodationPrefs, price_level);
            string selectedAccommodationPreference = GetSelectedAccommodationPreference(accommodationPrefs);
            var accommodation = await FindAndSelectHotelWithPreferences(
                destination, price_level, accommodationQueryPrefix, selectedAccommodationPreference, dislikedGoogleIds);

            for (int i = 0; i < dayCount; i++)
            {
                standardRoute.TravelDays[i].Accomodation = CreateAccommodation(accommodation, price_level);
            }

            var picker = new PlacePicker(_random, dislikedGoogleIds);
            var allUsedGoogleIds = picker.UsedGoogleIds;

            for (int day = 0; day < dayCount; day++)
            {
                string breakfastQuery = _placeQueryBuilder.Food(foodPrefs, price_level, MealType.Breakfast);
                standardRoute.TravelDays[day].Breakfast = await FindAndSelectMealWithPreferences<Breakfast>(
                    destination, price_level, breakfastQuery, day, picker);

                string lunchQuery = _placeQueryBuilder.Food(foodPrefs, price_level, MealType.Lunch);
                standardRoute.TravelDays[day].Lunch = await FindAndSelectMealWithPreferences<Lunch>(
                    destination, price_level, lunchQuery, day, picker);

                string dinnerQuery = _placeQueryBuilder.Food(foodPrefs, price_level, MealType.Dinner);
                standardRoute.TravelDays[day].Dinner = await FindAndSelectMealWithPreferences<Dinner>(
                    destination, price_level, dinnerQuery, day, picker);
            }

            string touristicQuery = _placeQueryBuilder.Touristic(personalizationPrefs);
            var touristicGoogleIds = (await FindTouristicPlacesWithPreferences(destination, touristicQuery, dayCount * 5)).ToList();

            if (touristicGoogleIds.Count < (dayCount * 5))
            {
                var additionalQuery = _placeQueryBuilder.AlternativeTouristic(personalizationPrefs, touristicQuery);
                var additionalPlaces = await FindMoreTouristicPlaces(destination, dayCount * 5, touristicGoogleIds, additionalQuery);
                touristicGoogleIds.AddRange(additionalPlaces);
            }

            touristicGoogleIds = touristicGoogleIds.Where(id => !allUsedGoogleIds.Contains(id)).ToList();

            string afterDinnerQuery = _placeQueryBuilder.AfterDinner(personalizationPrefs, price_level);
            var afterDinnerGoogleIds = (await FindAfterDinnerPlacesWithPreferences(destination, afterDinnerQuery, price_level)).ToList();

            afterDinnerGoogleIds = afterDinnerGoogleIds.Where(id => !allUsedGoogleIds.Contains(id)).ToList();

            if (touristicGoogleIds.Count < (dayCount * 3) || afterDinnerGoogleIds.Count < dayCount)
            {
                await EnsureEnoughUniquePlaces(destination, dayCount, touristicGoogleIds, afterDinnerGoogleIds, allUsedGoogleIds);
            }

            for (int day = 0; day < dayCount; day++)
            {
                int dayAttempt = 0;

                while (true)
                {
                try
                {
                    var selectedTouristicPlaces = await _placeSelectionService.SelectManyAsync<Domain.Entities.Route.Place>(
                        touristicGoogleIds, picker, 3, offset: day * 3);

                    foreach (var touristicPlace in selectedTouristicPlaces)
                    {
                        touristicPlace.UserPersonalizationPref = touristicQuery;
                    }

                    standardRoute.TravelDays[day].FirstPlace = selectedTouristicPlaces[0];
                    standardRoute.TravelDays[day].SecondPlace = selectedTouristicPlaces[1];
                    standardRoute.TravelDays[day].ThirdPlace = selectedTouristicPlaces[2];

                    var afterDinnerPlace = await _placeSelectionService.SelectAsync<PlaceAfterDinner>(
                        afterDinnerGoogleIds, picker, price_level, offset: day);

                    afterDinnerPlace.UserFoodPreference = afterDinnerQuery;

                    standardRoute.TravelDays[day].PlaceAfterDinner = afterDinnerPlace;
                    break;
                }
                catch (Exception ex)
                {
                    dayAttempt++;

                    if (dayAttempt > MaxDayRetries)
                    {
                        throw new RouteGenerationException(
                            $"Day {day + 1} in {destination} could not be filled with unique places.", ex);
                    }

                    _logger.LogWarning(
                        ex,
                        "Could not select places for day {Day}; widening the search ({Attempt}/{Max}).",
                        day + 1,
                        dayAttempt,
                        MaxDayRetries);

                    await GetMoreUniquePlaces(destination, touristicGoogleIds, afterDinnerGoogleIds, allUsedGoogleIds, price_level);
                }
                }
            }

            var validation = _routePlanValidator.Validate(standardRoute);

            if (!validation.IsValid)
            {
                throw new RouteGenerationException(
                    $"No usable route could be built for {destination} ({validation.Describe()}).");
            }

            await _routeEnrichmentService.ApplyAsync(standardRoute, startDate, endDate);

            await SaveRouteToDatabase(standardRoute);
            standardRoute.User = null;

            return standardRoute;
        }

        private async Task EnsureEnoughUniquePlaces(
            string destination,
            int dayCount,
            List<string> touristicGoogleIds,
            List<string> afterDinnerGoogleIds,
            IReadOnlyCollection<string> usedGoogleIds)
        {
            if (touristicGoogleIds.Count < dayCount * 3)
            {
                var queries = new[]
                {
                    "Must visit attractions",
                    "Top attractions",
                    "Things to do in",
                    "Popular sites",
                    "Tourist sites",
                    "Landmarks in",
                    "Famous places in"
                }.Select(query => $"{query} {destination}");

                touristicGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    queries,
                    dayCount * 4 - touristicGoogleIds.Count,
                    touristicGoogleIds.Concat(usedGoogleIds)));
            }

            if (afterDinnerGoogleIds.Count < dayCount)
            {
                var queries = new[]
                {
                    "Evening entertainment",
                    "Nightlife",
                    "Bars",
                    "Pubs",
                    "Cafes",
                    "Late night venues"
                }.Select(query => $"{query} in {destination}");

                afterDinnerGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                    queries,
                    dayCount * 2 - afterDinnerGoogleIds.Count,
                    afterDinnerGoogleIds.Concat(usedGoogleIds)));
            }
        }

        private async Task GetMoreUniquePlaces(
            string destination,
            List<string> touristicGoogleIds,
            List<string> afterDinnerGoogleIds,
            IReadOnlyCollection<string> usedGoogleIds,
            PRICE_LEVEL? priceLevel)
        {
            var touristicQueries = new[]
            {
                "Hidden gems in",
                "Off the beaten path in",
                "Unusual attractions in",
                "Secret spots in",
                "Lesser known attractions in",
                "Local favorites in"
            }.Select(query => $"{query} {destination}");

            touristicGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                touristicQueries,
                excludedPlaceIds: touristicGoogleIds.Concat(usedGoogleIds)));

            var afterDinnerQuery = _placeQueryBuilder.AlternativeAfterDinner(priceLevel);

            afterDinnerGoogleIds.AddRange(await _placeSearchService.CollectPlaceIdsAsync(
                new[] { $"{afterDinnerQuery} in {destination}" },
                excludedPlaceIds: afterDinnerGoogleIds.Concat(usedGoogleIds)));
        }

        private async Task<T> FindAndSelectMealWithPreferences<T>(
            string destination,
            PRICE_LEVEL? priceLevel,
            string foodQueryPrefix,
            int dayIndex,
            PlacePicker picker) where T : class, ISelectablePlace, IHasFoodPreference, new()
        {
            string mealQuery = $"{foodQueryPrefix} in {destination}";

            MealType mealType = typeof(T).Name switch
            {
                nameof(Breakfast) => MealType.Breakfast,
                nameof(Lunch) => MealType.Lunch,
                nameof(Dinner) => MealType.Dinner,
                _ => MealType.Breakfast
            };

            string simplifiedQuery = mealType switch
            {
                MealType.Breakfast => $"{_placeQueryBuilder.PricePrefix(priceLevel)}{BREAKFAST_QUERY} in {destination}",
                MealType.Lunch => $"{_placeQueryBuilder.PricePrefix(priceLevel)}{LUNCH_QUERY} in {destination}",
                MealType.Dinner => $"{_placeQueryBuilder.PricePrefix(priceLevel)}{DINNER_QUERY} in {destination}",
                _ => $"restaurant in {destination}"
            };

            var mealList = await _placeSearchService.SearchFirstMatchAsync(new[]
            {
                mealQuery,
                simplifiedQuery,
                $"restaurant in {destination}"
            });

            if (mealList.Count == 0)
            {
                throw new InvalidOperationException($"No suitable meal places found in {destination}");
            }

            var uniquePlaces = mealList
                .Where(p => p?.Id != null && !picker.IsUsed(p.Id))
                .ToList();

            if (uniquePlaces.Count == 0)
            {
                var alternativeQuery = _placeQueryBuilder.AlternativeFood(mealType, priceLevel);
                var alternateMealList = await _placeSearchService.SearchAsync($"{alternativeQuery} in {destination}");

                uniquePlaces = alternateMealList
                    .Where(p => p?.Id != null && !picker.IsUsed(p.Id))
                    .ToList();

                if (uniquePlaces.Count == 0)
                {
                    uniquePlaces = mealList
                        .Where(p => p?.Id != null)
                        .ToList();

                    _logger.LogWarning("Could not find unique {PlaceType} places; some places may be reused.", typeof(T).Name);
                }
            }

            int indexOffset = dayIndex % Math.Max(1, uniquePlaces.Count);
            int selectIndex = (_random.Next(uniquePlaces.Count) + indexOffset) % uniquePlaces.Count;

            var selectedPlace = uniquePlaces[selectIndex];

            var mealPlace = await _placeSelectionService.MaterializeAsync<T>(selectedPlace.Id!, priceLevel);
            mealPlace.UserFoodPreference = foodQueryPrefix;

            picker.MarkUsed(selectedPlace.Id!);

            return mealPlace;
        }

        private Task<IReadOnlyList<string>> FindTouristicPlacesWithPreferences(
            string destination,
            string touristicQuery,
            int requiredCount)
        {
            var queries = new List<string> { $"{touristicQuery} in {destination}" };

            queries.AddRange(new[]
            {
                TOURISTIC_QUERY,
                TOURISTIC_QUERY1,
                TOURISTIC_QUERY2,
                TOURISTIC_QUERY3,
                TOURISTIC_QUERY4
            }.Select(query => $"{query} in {destination}"));

            return _placeSearchService.CollectPlaceIdsAsync(queries, requiredCount);
        }

        private Task<IReadOnlyList<string>> FindMoreTouristicPlaces(
            string destination,
            int requiredCount,
            IReadOnlyCollection<string> existingIds,
            string additionalQuery)
        {
            var queries = new List<string> { $"{additionalQuery} in {destination}" };

            queries.AddRange(new[]
            {
                "Famous tourist attractions",
                "Must-see places",
                "Popular tourist destinations",
                "Cultural attractions",
                "Historical sites"
            }.Select(query => $"{query} in {destination}"));

            return _placeSearchService.CollectPlaceIdsAsync(
                queries,
                Math.Max(0, requiredCount - existingIds.Count),
                existingIds);
        }

        private Task<IReadOnlyList<string>> FindAfterDinnerPlacesWithPreferences(
            string destination,
            string afterDinnerQuery,
            PRICE_LEVEL? priceLevel)
        {
            var queries = new List<string> { $"{afterDinnerQuery} in {destination}" };

            queries.AddRange(new[]
            {
                AFTER_DINNER_QUERY1,
                AFTER_DINNER_QUERY2,
                AFTER_DINNER_QUERY3,
                AFTER_DINNER_QUERY4,
                AFTER_DINNER_QUERY5
            }.Select(query => $"{_placeQueryBuilder.PricePrefix(priceLevel)}{query} in {destination}"));

            return _placeSearchService.CollectPlaceIdsAsync(queries);
        }

        private async Task<List<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>> GatherPreferencesAsync(
            AppUser user, string username, List<string>? friends)
        {
            var travellers = new List<AppUser> { user };

            foreach (var friendUsername in friends ?? new List<string>())
            {
                if (string.Equals(friendUsername, username, StringComparison.OrdinalIgnoreCase) ||
                    travellers.Any(traveller => traveller.UserName == friendUsername))
                {
                    continue;
                }

                var friend = await _userManager.FindByNameAsync(friendUsername);

                if (friend != null && await _friendshipService.AreFriendsAsync(username, friendUsername))
                {
                    travellers.Add(friend);
                }
            }

            var ids = travellers.Select(traveller => traveller.Id).ToList();

            var accommodation = await _context.Set<UserAccommodationPreferences>()
                .Where(preference => ids.Contains(preference.UserId))
                .ToDictionaryAsync(preference => preference.UserId);

            var food = await _context.Set<UserFoodPreferences>()
                .Where(preference => ids.Contains(preference.UserId))
                .ToDictionaryAsync(preference => preference.UserId);

            var personalization = await _context.Set<UserPersonalization>()
                .Where(preference => ids.Contains(preference.UserId))
                .ToDictionaryAsync(preference => preference.UserId);

            var gathered = new List<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>();

            foreach (var id in ids)
            {
                if (accommodation.TryGetValue(id, out var accommodationPreference) &&
                    food.TryGetValue(id, out var foodPreference) &&
                    personalization.TryGetValue(id, out var personalizationPreference))
                {
                    gathered.Add((accommodationPreference, foodPreference, personalizationPreference));
                }
            }

            return gathered;
        }

        private static void ValidateParameters(string? destination, DateOnly? startDate, DateOnly? endDate, string? username)
        {
            ValidateParameters(destination, startDate, endDate);

            if (string.IsNullOrWhiteSpace(username))
                throw new ArgumentNullException(nameof(username));
        }

        private string GetSelectedAccommodationPreference(IReadOnlyList<PreferenceItem> accommodationPrefs)
        {
            if (accommodationPrefs == null || accommodationPrefs.Count == 0)
            {
                return "No specific preference";
            }

            string selectedPreference = _placeQueryBuilder.SelectPreference(accommodationPrefs);

            foreach (AccommodationPreferenceTypes type in Enum.GetValues(typeof(AccommodationPreferenceTypes)))
            {
                var propertyName = $"{type}Preference";
                if (propertyName == selectedPreference)
                {
                    var description = type.GetDescription();
                    return description;
                }
            }

            return selectedPreference;
        }

        private async Task<TravelAccomodation> FindAndSelectHotelWithPreferences(
            string destination,
            PRICE_LEVEL? priceLevel,
            string accommodationQueryPrefix,
            string selectedAccommodationPreference,
            IReadOnlyCollection<string> excludedGoogleIds)
        {
            string hotelQuery = $"{accommodationQueryPrefix} in {destination}";
            var hotelList = (await _placeSearchService.SearchFirstMatchAsync(new[]
            {
                hotelQuery,
                $"{_placeQueryBuilder.HotelStars(priceLevel)} star hotel in {destination}",
                $"hotel in {destination}"
            })).Where(hotel => !excludedGoogleIds.Contains(hotel.Id!)).ToList();

            if (hotelList.Count == 0)
            {
                throw new InvalidOperationException($"No suitable hotels found in {destination}");
            }

            int randomIndex = _random.Next(hotelList.Count);
            var placeId = hotelList[randomIndex].Id;

            var hotel = await _placeSelectionService.MaterializeAsync<TravelAccomodation>(placeId!, priceLevel);
            hotel.UserAccomodationPreference = selectedAccommodationPreference;

            return hotel;
        }

    }
}
