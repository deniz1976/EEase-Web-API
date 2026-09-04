using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Exceptions;
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
        private readonly IGeminiAIService _geminiAIService;
        private readonly IGooglePlacesService _googlePlacesService;
        private readonly EEaseAPIDbContext _context;
        private readonly ISystemUserProvider _systemUserProvider;
        private readonly ILogger<CustomRouteService> _logger;

        private readonly static string EXPENSIVE_HOTEL_QUERY = "5";
        private readonly static string MODERATE_HOTEL_QUERY = "4";
        private readonly static string INEXPENSIVE_HOTEL_QUERY = "3";
        private readonly static string VERY_EXPENSIVE_HOTEL_QUERY = "5";

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
        private readonly static string VERY_EXPENSIVE_QUERY = "Very expensive ";

        private readonly static string TOURISTIC_QUERY = "Touristic places ";
        private readonly static string TOURISTIC_QUERY1 = "Historic landmarks and museums ";
        private readonly static string TOURISTIC_QUERY2 = "Hidden gem sightseeing spots ";
        private readonly static string TOURISTIC_QUERY3 = "Breathtaking natural attractions ";
        private readonly static string TOURISTIC_QUERY4 = "Off the beaten path tourist spots ";

        private readonly static int MAX_RETRY_ATTEMPTS = 50;

        private readonly static HashSet<string> EXCLUDED_PLACE_TYPES = new HashSet<string>
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

        public CustomRouteService(
            UserManager<AppUser> userManager,
            IGooglePlacesService googlePlacesService,
            EEaseAPIDbContext context,
            IGeminiAIService geminiAIService,
            ISystemUserProvider systemUserProvider,
            ILogger<CustomRouteService> logger)
        {
            _userManager = userManager;
            _geminiAIService = geminiAIService;
            _googlePlacesService = googlePlacesService;
            _context = context;
            _systemUserProvider = systemUserProvider;
            _logger = logger;
        }

        public async Task<StandardRoute> CreateRandomRoute(string destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL)
        {
            ValidateParameters(destination, startDate, endDate);

            Random random = new Random();
            destination = FormatDestination(destination);

            var dayCount = CalculateDayCount(startDate, endDate);

            var admin = await GetAdminUser();

            int maxAttempts = 3;
            StandardRoute standardRoute = null;
            bool isValid = false;

            for (int attempt = 0; attempt < maxAttempts && !isValid; attempt++)
            {
                standardRoute = InitializeStandardRoute(destination, dayCount, admin.Id);

                var accomodation = await FindAndSelectHotel(destination, _PRICE_LEVEL, random);

                string priceQueryPrefix = GetPriceQueryPrefix(_PRICE_LEVEL);
                var (breakfastPlaces, lunchPlaces, dinnerPlaces) = await FindFoodPlaces(destination, priceQueryPrefix, dayCount, _PRICE_LEVEL);

                var touristicGoogleIds = await FindTouristicPlaces(destination, dayCount * 3);

                if (touristicGoogleIds.Count < (dayCount * 3))
                {
                    var additionalPlaces = await FindMoreTouristicPlaces(destination, dayCount * 3, touristicGoogleIds);
                    touristicGoogleIds.AddRange(additionalPlaces);
                }

                var afterDinnerGoogleIds = await FindAfterDinnerPlaces(destination, priceQueryPrefix, dayCount, _PRICE_LEVEL);

                var breakfastGoogleIds = breakfastPlaces.Places.Select(p => p.Id).ToList();
                var lunchGoogleIds = lunchPlaces.Places.Select(p => p.Id).ToList();
                var dinnerGoogleIds = dinnerPlaces.Places.Select(p => p.Id).ToList();

                try
                {
                    EnsureEnoughPlaces(breakfastPlaces.Places.Count, lunchPlaces.Places.Count, dinnerPlaces.Places.Count, touristicGoogleIds.Count, dayCount);

                    var usedBreakfastIndices = new HashSet<int>();
                    var usedLunchIndices = new HashSet<int>();
                    var usedDinnerIndices = new HashSet<int>();
                    var usedAfterDinnerIndices = new HashSet<int>();
                    var usedTouristicIndices = new HashSet<int>();
                    var usedGoogleIds = new HashSet<string>();

                    for (int i = 0; i < dayCount; i++)
                    {
                        var day = new TravelDay();

                        day.Accomodation = CreateAccommodation(accomodation, i, _PRICE_LEVEL);

                        day.Breakfast = await SelectUniquePlace<Breakfast>(
                            breakfastPlaces, breakfastGoogleIds, usedBreakfastIndices, usedGoogleIds, random, _PRICE_LEVEL);

                        day.Lunch = await SelectUniquePlace<Lunch>(
                            lunchPlaces, lunchGoogleIds, usedLunchIndices, usedGoogleIds, random, _PRICE_LEVEL);

                        day.Dinner = await SelectUniquePlace<Dinner>(
                            dinnerPlaces, dinnerGoogleIds, usedDinnerIndices, usedGoogleIds, random, _PRICE_LEVEL);

                        day.PlaceAfterDinner = await SelectUniqueAfterDinnerPlace(
                            afterDinnerGoogleIds, usedAfterDinnerIndices, usedGoogleIds, random, _PRICE_LEVEL);

                        var (firstPlace, secondPlace, thirdPlace) = await SelectThreeTouristicPlaces(
                            touristicGoogleIds, usedTouristicIndices, usedGoogleIds, random);

                        day.FirstPlace = firstPlace;
                        day.SecondPlace = secondPlace;
                        day.ThirdPlace = thirdPlace;

                        standardRoute.TravelDays[i] = day;
                    }

                    isValid = RouteTest(standardRoute);

                    if (!isValid && attempt < maxAttempts - 1)
                    {
                        _logger.LogWarning("The generated route contains duplicate places. Attempt: {Attempt}", attempt + 1);
                    }
                }
                catch (InvalidOperationException ex)
                {
                    _logger.LogWarning(ex, "Could not create the route. Attempt: {Attempt}", attempt + 1);
                    if (attempt == maxAttempts - 1)
                    {
                        throw;
                    }
                }
            }

            var response = await GetGeminiDataForRouteAsync(standardRoute, startDate, endDate);
            
            await SaveRouteToDatabase(standardRoute);
            standardRoute.User = null;

            return standardRoute;
        }

        #region Helper Methods

        private void ValidateParameters(string destination, DateOnly? startDate, DateOnly? endDate)
        {
            if (destination == null || startDate == null || endDate == null)
                throw new ArgumentNullException();
        }

        private string FormatDestination(string destination)
        {
            return char.ToUpper(destination[0]) + destination[1..].ToLowerInvariant();
        }

        private int CalculateDayCount(DateOnly? startDate, DateOnly? endDate)
        {
            var dayCount = (endDate.Value.DayNumber - startDate.Value.DayNumber) + 1;
            return dayCount;
        }

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
                Status = 2
            };

            for (int i = 0; i < dayCount; i++)
            {
                standardRoute.TravelDays.Add(new TravelDay());
            }

            return standardRoute;
        }

        private async Task<TravelAccomodation> FindAndSelectHotel(string destination, PRICE_LEVEL? priceLevel, Random random)
        {
            string queryPrefix = GetHotelQueryPrefix(priceLevel);
            string hotelQuery = $"{queryPrefix} star hotel in {destination}";

            var hotelList = await _googlePlacesService.SearchPlacesAsync(hotelQuery);

            if (hotelList?.Places != null)
                hotelList.Places = FilterUnwantedPlaceTypes(hotelList.Places);

            if (hotelList?.Places == null || hotelList.Places.Count == 0)
            {
                hotelList = await _googlePlacesService.SearchPlacesAsync(hotelQuery);
            }

            int select = random.Next(hotelList.Places.Count);
            var hotel = hotelList?.Places?[select];

            var hotelDetails = await _googlePlacesService.GetPlaceDetailsAsync(hotel.Id);
            TravelAccomodation accomodation = JsonConvert.DeserializeObject<TravelAccomodation>(hotelDetails);
            accomodation.Id = Guid.NewGuid();
            accomodation.GoogleId = hotel.Id;
            accomodation._PRICE_LEVEL = priceLevel;
            accomodation.Star = queryPrefix;
            accomodation.UserAccomodationPreference = "random";

            return accomodation;
        }

        private string GetHotelQueryPrefix(PRICE_LEVEL? priceLevel)
        {
            return priceLevel switch
            {
                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => EXPENSIVE_HOTEL_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_MODERATE => MODERATE_HOTEL_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => INEXPENSIVE_HOTEL_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => VERY_EXPENSIVE_HOTEL_QUERY,
                _ => EXPENSIVE_HOTEL_QUERY
            };
        }

        private string GetPriceQueryPrefix(PRICE_LEVEL? priceLevel)
        {
            return priceLevel switch
            {
                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => EXPENSIVE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_MODERATE => MODERATE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => INEXPENSIVE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => VERY_EXPENSIVE_QUERY,
                _ => MODERATE_QUERY
            };
        }

        private async Task<(PlaceSearchResponse breakfastPlaces, PlaceSearchResponse lunchPlaces, PlaceSearchResponse dinnerPlaces)>
            FindFoodPlaces(string destination, string priceQueryPrefix, int dayCount, PRICE_LEVEL? priceLevel)
        {
            var breakfastPlaces = await _googlePlacesService.SearchPlacesAsync($"{priceQueryPrefix} {BREAKFAST_QUERY} in {destination}")
                ?? new PlaceSearchResponse { Places = new List<Application.DTOs.GooglePlaces.Place>() };
            var lunchPlaces = await _googlePlacesService.SearchPlacesAsync($"{priceQueryPrefix} {LUNCH_QUERY} in {destination}")
                ?? new PlaceSearchResponse { Places = new List<Application.DTOs.GooglePlaces.Place>() };
            var dinnerPlaces = await _googlePlacesService.SearchPlacesAsync($"{priceQueryPrefix} {DINNER_QUERY} in {destination}")
                ?? new PlaceSearchResponse { Places = new List<Application.DTOs.GooglePlaces.Place>() };

            breakfastPlaces.Places ??= new List<Application.DTOs.GooglePlaces.Place>();
            lunchPlaces.Places ??= new List<Application.DTOs.GooglePlaces.Place>();
            dinnerPlaces.Places ??= new List<Application.DTOs.GooglePlaces.Place>();

            breakfastPlaces.Places = FilterUnwantedPlaceTypes(breakfastPlaces.Places);
            lunchPlaces.Places = FilterUnwantedPlaceTypes(lunchPlaces.Places);
            dinnerPlaces.Places = FilterUnwantedPlaceTypes(dinnerPlaces.Places);

            if (breakfastPlaces.Places.Count < dayCount)
            {
                var additionalPrefix = GetNextPriceLevel(priceQueryPrefix, priceLevel);
                var additionalBreakfastPlaces = await _googlePlacesService.SearchPlacesAsync($"{additionalPrefix}{BREAKFAST_QUERY}" +
                    $"in {destination}");
                if (additionalBreakfastPlaces?.Places != null)
                {
                    additionalBreakfastPlaces.Places = FilterUnwantedPlaceTypes(additionalBreakfastPlaces.Places);
                    breakfastPlaces.Places.AddRange(additionalBreakfastPlaces.Places);
                }
            }

            if (lunchPlaces.Places.Count < dayCount)
            {
                var additionalPrefix = GetNextPriceLevel(priceQueryPrefix, priceLevel);
                var additionalLunchPlaces = await _googlePlacesService.SearchPlacesAsync($"{additionalPrefix}{LUNCH_QUERY}" +
                    $"in {destination}");
                if (additionalLunchPlaces?.Places != null)
                {
                    additionalLunchPlaces.Places = FilterUnwantedPlaceTypes(additionalLunchPlaces.Places);
                    lunchPlaces.Places.AddRange(additionalLunchPlaces.Places);
                }
            }

            if (dinnerPlaces.Places.Count < dayCount)
            {
                var additionalPrefix = GetNextPriceLevel(priceQueryPrefix, priceLevel);
                var additionalDinnerPlaces = await _googlePlacesService.SearchPlacesAsync($"{additionalPrefix}{DINNER_QUERY}" +
                    $"in {destination}");
                if (additionalDinnerPlaces?.Places != null)
                {
                    additionalDinnerPlaces.Places = FilterUnwantedPlaceTypes(additionalDinnerPlaces.Places);
                    dinnerPlaces.Places.AddRange(additionalDinnerPlaces.Places);
                }
            }

            return (breakfastPlaces, lunchPlaces, dinnerPlaces);
        }

        private async Task<List<string>> FindTouristicPlaces(string destination, int requiredCount = 0)
        {
            var touristicQueries = new[] {
                TOURISTIC_QUERY,
                TOURISTIC_QUERY1,
                TOURISTIC_QUERY2,
                TOURISTIC_QUERY3,
                TOURISTIC_QUERY4,
                "Famous tourist attractions in ",
                "Must visit spots in ",
                "Top rated attractions in "
            };

            var allTouristicPlaces = new List<PlaceSearchResponse>();

            foreach (var query in touristicQueries)
            {
                var places = await _googlePlacesService.SearchPlacesAsync($"{query}in {destination}");
                if (places?.Places != null)
                {
                    places.Places = FilterUnwantedPlaceTypes(places.Places);

                    allTouristicPlaces.Add(places);
                }

                if (requiredCount > 0)
                {
                    var totalCount = allTouristicPlaces.SelectMany(p => p.Places).Select(p => p.Id).Distinct().Count();
                    if (totalCount >= requiredCount)
                        break;
                }
            }

            return allTouristicPlaces
                .SelectMany(p => p.Places ?? new List<Application.DTOs.GooglePlaces.Place>())
                .Where(p => !string.IsNullOrEmpty(p?.Id))
                .Select(p => p!.Id!)
                .Distinct()
                .ToList();
        }

        private async Task<List<string>> FindMoreTouristicPlaces(string destination, int requiredCount, List<string> existingIds)
        {
            var additionalQueries = new[] {
                "Famous tourist attractions in ",
                "Must-see places in ",
                "Popular tourist destinations in ",
                "Top-rated places to visit in ",
                "Cultural attractions in ",
                "Historical sites in "
            };

            var additionalIds = new List<string>();

            foreach (var query in additionalQueries)
            {
                if (existingIds.Count + additionalIds.Count >= requiredCount) break;

                var places = await _googlePlacesService.SearchPlacesAsync($"{query}{destination}");
                if (places?.Places != null)
                {
                    places.Places = FilterUnwantedPlaceTypes(places.Places);

                    var newIds = places.Places.Select(p => p.Id)
                        .Where(id => !existingIds.Contains(id) && !additionalIds.Contains(id))
                        .ToList();

                    additionalIds.AddRange(newIds);
                }
            }

            return additionalIds;
        }

        private async Task<List<string>> FindAfterDinnerPlaces(string destination, string priceQueryPrefix, int dayCount, PRICE_LEVEL? priceLevel)
        {
            var afterDinnerQueries = new[] { AFTER_DINNER_QUERY1, AFTER_DINNER_QUERY2, AFTER_DINNER_QUERY3, AFTER_DINNER_QUERY4, AFTER_DINNER_QUERY5 };
            var afterDinnerGoogleIds = new List<string>();

            foreach (var query in afterDinnerQueries)
            {
                var places = await _googlePlacesService.SearchPlacesAsync($"{priceQueryPrefix} {query} in {destination}");
                if (places?.Places != null)
                {
                    places.Places = FilterUnwantedPlaceTypes(places.Places);
                    var validIds = places.Places
                        .Where(p => !string.IsNullOrEmpty(p?.Id))
                        .Select(p => p!.Id!)
                        .ToList();
                    afterDinnerGoogleIds.AddRange(validIds);
                }
            }

            return afterDinnerGoogleIds;
        }

        private void EnsureEnoughPlaces(int breakfastCount, int lunchCount, int dinnerCount, int touristicCount, int dayCount)
        {
            if (breakfastCount < dayCount ||
                lunchCount < dayCount ||
                dinnerCount < dayCount ||
                touristicCount < (dayCount * 3))
            {
                throw new InvalidOperationException("Not enough unique places found even after trying different price levels.");
            }
        }

        private TravelAccomodation CreateAccommodation(TravelAccomodation baseAccommodation, int dayIndex, PRICE_LEVEL? priceLevel)
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

        private async Task<T> SelectUniquePlace<T>(
            PlaceSearchResponse placesResponse,
            List<string> googleIds,
            HashSet<int> usedIndices,
            HashSet<string> usedGoogleIds,
            Random random,
            PRICE_LEVEL? priceLevel) where T : class, new()
        {
            if (placesResponse?.Places == null || !placesResponse.Places.Any() || googleIds == null || !googleIds.Any())
            {
                throw new InvalidOperationException("No valid places found");
            }

            var availableIndices = new List<int>();
            for (int i = 0; i < placesResponse.Places.Count; i++)
            {
                if (!usedIndices.Contains(i) && !usedGoogleIds.Contains(googleIds[i]))
                {
                    availableIndices.Add(i);
                }
            }

            int index;

            if (availableIndices.Count > 0)
            {
                index = availableIndices[random.Next(availableIndices.Count)];
            }
            else
            {
                int retryCount = 0;
                index = random.Next(placesResponse.Places.Count);

                while (usedGoogleIds.Contains(googleIds[index]) && retryCount < MAX_RETRY_ATTEMPTS)
                {
                    index = random.Next(placesResponse.Places.Count);
                    retryCount++;
                }

                if (retryCount >= MAX_RETRY_ATTEMPTS)
                {
                    retryCount = 0;
                    while (usedIndices.Contains(index) && retryCount < MAX_RETRY_ATTEMPTS)
                    {
                        index = random.Next(placesResponse.Places.Count);
                        retryCount++;
                    }
                }

                if (retryCount >= MAX_RETRY_ATTEMPTS)
                {
                    index = random.Next(placesResponse.Places.Count);
                }
            }

            usedIndices.Add(index);
            usedGoogleIds.Add(googleIds[index]);

            var placeDetails = await _googlePlacesService.GetPlaceDetailsAsync(googleIds[index]);
            var place = JsonConvert.DeserializeObject<T>(placeDetails);

            typeof(T).GetProperty("Id")?.SetValue(place, Guid.NewGuid());
            typeof(T).GetProperty("_PRICE_LEVEL")?.SetValue(place, priceLevel);
            typeof(T).GetProperty("GoogleId")?.SetValue(place, googleIds[index]);

            return place;
        }

        private async Task<PlaceAfterDinner> SelectUniqueAfterDinnerPlace(
            List<string> afterDinnerGoogleIds,
            HashSet<int> usedIndices,
            HashSet<string> usedGoogleIds,
            Random random,
            PRICE_LEVEL? priceLevel)
        {
            var availableIndices = new List<int>();
            for (int i = 0; i < afterDinnerGoogleIds.Count; i++)
            {
                if (!usedIndices.Contains(i) && !usedGoogleIds.Contains(afterDinnerGoogleIds[i]))
                {
                    availableIndices.Add(i);
                }
            }

            int index;

            if (availableIndices.Count > 0)
            {
                index = availableIndices[random.Next(availableIndices.Count)];
            }
            else
            {
                int retryCount = 0;
                index = random.Next(afterDinnerGoogleIds.Count);

                while (usedGoogleIds.Contains(afterDinnerGoogleIds[index]) && retryCount < MAX_RETRY_ATTEMPTS)
                {
                    index = random.Next(afterDinnerGoogleIds.Count);
                    retryCount++;
                }

                if (retryCount >= MAX_RETRY_ATTEMPTS)
                {
                    retryCount = 0;
                    while (usedIndices.Contains(index) && retryCount < MAX_RETRY_ATTEMPTS)
                    {
                        index = random.Next(afterDinnerGoogleIds.Count);
                        retryCount++;
                    }
                }

                if (retryCount >= MAX_RETRY_ATTEMPTS)
                {
                    index = random.Next(afterDinnerGoogleIds.Count);
                }
            }

            usedIndices.Add(index);
            usedGoogleIds.Add(afterDinnerGoogleIds[index]);

            var placeDetails = await _googlePlacesService.GetPlaceDetailsAsync(afterDinnerGoogleIds[index]);
            var place = JsonConvert.DeserializeObject<PlaceAfterDinner>(placeDetails);

            place.Id = Guid.NewGuid();
            place._PRICE_LEVEL = priceLevel;
            place.GoogleId = afterDinnerGoogleIds[index];

            return place;
        }

        private async Task<(Domain.Entities.Route.Place firstPlace, Domain.Entities.Route.Place secondPlace, Domain.Entities.Route.Place thirdPlace)>
            SelectThreeTouristicPlaces(
                List<string> touristicGoogleIds, 
                HashSet<int> usedIndices, 
                HashSet<string> usedGoogleIds, 
                Random random)
        {
            int firstPlaceIndex = await GetUniqueIndex(touristicGoogleIds, usedIndices, usedGoogleIds, random);
            var firstPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[firstPlaceIndex]);
            var firstPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(firstPlaceDetails);
            firstPlace.Id = Guid.NewGuid();
            firstPlace.GoogleId = touristicGoogleIds[firstPlaceIndex];

            int secondPlaceIndex = await GetUniqueIndex(touristicGoogleIds, usedIndices, usedGoogleIds, random);
            var secondPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[secondPlaceIndex]);
            var secondPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(secondPlaceDetails);
            secondPlace.Id = Guid.NewGuid();
            secondPlace.GoogleId = touristicGoogleIds[secondPlaceIndex];

            int thirdPlaceIndex = await GetUniqueIndex(touristicGoogleIds, usedIndices, usedGoogleIds, random);
            var thirdPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[thirdPlaceIndex]);
            var thirdPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(thirdPlaceDetails);
            thirdPlace.Id = Guid.NewGuid();
            thirdPlace.GoogleId = touristicGoogleIds[thirdPlaceIndex];

            return (firstPlace, secondPlace, thirdPlace);
        }

        private async Task<int> GetUniqueIndex(List<string> googleIds, HashSet<int> usedIndices, HashSet<string> usedGoogleIds, Random random)
        {
            var availableIndices = new List<int>();
            for (int i = 0; i < googleIds.Count; i++)
            {
                if (!usedIndices.Contains(i) && !usedGoogleIds.Contains(googleIds[i]))
                {
                    availableIndices.Add(i);
                }
            }

            int index;

            if (availableIndices.Count > 0)
            {
                index = availableIndices[random.Next(availableIndices.Count)];
            }
            else
            {
                for (int i = 0; i < googleIds.Count; i++)
                {
                    if (!usedGoogleIds.Contains(googleIds[i]))
                    {
                        index = i;
                        usedIndices.Add(index);
                        usedGoogleIds.Add(googleIds[index]);
                        return index;
                    }
                }

                index = random.Next(googleIds.Count);
            }

            usedIndices.Add(index);
            usedGoogleIds.Add(googleIds[index]);

            return index;
        }

        private async Task SaveRouteToDatabase(StandardRoute route)
        {
            await _context.StandardRoutes.AddAsync(route);
            await _context.SaveChangesAsync();
        }

        private string GetNextPriceLevel(string currentPrefix, PRICE_LEVEL? currentLevel)
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

        private List<Application.DTOs.GooglePlaces.Place> FilterUnwantedPlaceTypes(List<Application.DTOs.GooglePlaces.Place> places)
        {
            if (places == null) return new List<Application.DTOs.GooglePlaces.Place>();

            return places.Where(place =>
                place?.Types == null ||
                !place.Types.Any() ||
                !place.Types.Any(type => EXCLUDED_PLACE_TYPES.Contains(type)))
                .ToList();
        }

        #endregion

        private bool RouteTest(StandardRoute? route)
        {
            if (route?.TravelDays == null || !route.TravelDays.Any())
                return false;

            int lengthShouldBe = route.TravelDays.Count * 7;
            var hashmap = new Dictionary<string, int>();

            try
            {
                foreach (var day in route.TravelDays)
                {
                    if (day == null) continue;

                    if (!string.IsNullOrEmpty(day.Breakfast?.GoogleId))
                        hashmap[day.Breakfast.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(day.Lunch?.GoogleId))
                        hashmap[day.Lunch.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(day.Dinner?.GoogleId))
                        hashmap[day.Dinner.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(day.FirstPlace?.GoogleId))
                        hashmap[day.FirstPlace.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(day.SecondPlace?.GoogleId))
                        hashmap[day.SecondPlace.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(day.ThirdPlace?.GoogleId))
                        hashmap[day.ThirdPlace.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(day.PlaceAfterDinner?.GoogleId))
                        hashmap[day.PlaceAfterDinner.GoogleId] = 1;
                }

                return hashmap.Count >= lengthShouldBe;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "An error occurred while validating the route.");
                return false;
            }
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

            var userPreferences = await GetUserPreferencesAsync(user);
            
            var allPreferences = new List<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>();
            
            if (userPreferences.Item1 != null && userPreferences.Item2 != null && userPreferences.Item3 != null)
            {
                allPreferences.Add((userPreferences.Item1, userPreferences.Item2, userPreferences.Item3));
            }
            
            if (friends != null && friends.Count > 0)
            {
                foreach (var friendUsername in friends)
                {
                    var friend = await _userManager.FindByNameAsync(friendUsername);
                    if (friend == null)
                    {
                        continue; 
                    }
                    
                    var areFriends = await IsFriendsAsync(user.Id, friend.Id);
                    if (!areFriends)
                    {
                        continue; 
                    }
                    
                    var friendPreferences = await GetUserPreferencesAsync(friend);
                    
                    if (friendPreferences.Item1 != null && friendPreferences.Item2 != null && friendPreferences.Item3 != null)
                    {
                        allPreferences.Add((friendPreferences.Item1, friendPreferences.Item2, friendPreferences.Item3));
                    }
                }
            }
            
            var (foodPrefs, accommodationPrefs, personalizationPrefs) = GatherAllPreferences(allPreferences);
            
            var standardRoute = InitializeStandardRoute(destination, dayCount, user.Id);
            
            Random random = new Random();
            
            string accommodationQueryPrefix = GetAccommodationQueryPrefix(accommodationPrefs, price_level);
            string selectedAccommodationPreference = GetSelectedAccommodationPreference(accommodationPrefs);
            var accommodation = await FindAndSelectHotelWithPreferences(destination, price_level, accommodationQueryPrefix, random, selectedAccommodationPreference);
            
            for (int i = 0; i < dayCount; i++)
            {
                standardRoute.TravelDays[i].Accomodation = CreateAccommodation(accommodation, i, price_level);
            }
            
            var allUsedGoogleIds = new HashSet<string>();
            
            for (int day = 0; day < dayCount; day++)
            {
                string breakfastQuery = GetFoodQueryPrefix(foodPrefs, price_level, MealType.Breakfast);
                standardRoute.TravelDays[day].Breakfast = await FindAndSelectMealWithPreferences<Breakfast>(
                    destination, price_level, breakfastQuery, random, day, allUsedGoogleIds);
                    
                string lunchQuery = GetFoodQueryPrefix(foodPrefs, price_level, MealType.Lunch);
                standardRoute.TravelDays[day].Lunch = await FindAndSelectMealWithPreferences<Lunch>(
                    destination, price_level, lunchQuery, random, day, allUsedGoogleIds);
                
                string dinnerQuery = GetFoodQueryPrefix(foodPrefs, price_level, MealType.Dinner);
                standardRoute.TravelDays[day].Dinner = await FindAndSelectMealWithPreferences<Dinner>(
                    destination, price_level, dinnerQuery, random, day, allUsedGoogleIds);
            }
            
            string touristicQuery = GetTouristicPlacesQueryPrefix(personalizationPrefs);
            var touristicGoogleIds = await FindTouristicPlacesWithPreferences(destination, touristicQuery, dayCount * 5); 
            
            if (touristicGoogleIds.Count < (dayCount * 5))
            {
                var additionalQuery = GetAdditionalTouristicQuery(personalizationPrefs, touristicQuery);
                var additionalPlaces = await FindMoreTouristicPlaces(destination, dayCount * 5, touristicGoogleIds, additionalQuery);
                touristicGoogleIds.AddRange(additionalPlaces);
            }
            
            touristicGoogleIds = touristicGoogleIds.Where(id => !allUsedGoogleIds.Contains(id)).ToList();
            
            string afterDinnerQuery = GetAfterDinnerQueryPrefix(personalizationPrefs, price_level);
            var afterDinnerGoogleIds = await FindAfterDinnerPlacesWithPreferences(destination, afterDinnerQuery, price_level);
            
            afterDinnerGoogleIds = afterDinnerGoogleIds.Where(id => !allUsedGoogleIds.Contains(id)).ToList();
            
            if (touristicGoogleIds.Count < (dayCount * 3) || afterDinnerGoogleIds.Count < dayCount)
            {
                await EnsureEnoughUniquePlaces(destination, dayCount, touristicGoogleIds, afterDinnerGoogleIds, allUsedGoogleIds);
            }
            
            for (int day = 0; day < dayCount; day++)
            {
                try
                {
                    var selectedTouristicPlaces = await SelectThreeUniqueTouristicPlaces(
                        touristicGoogleIds, allUsedGoogleIds, random, day, touristicQuery);
                    
                    standardRoute.TravelDays[day].FirstPlace = selectedTouristicPlaces.Item1;
                    standardRoute.TravelDays[day].SecondPlace = selectedTouristicPlaces.Item2;
                    standardRoute.TravelDays[day].ThirdPlace = selectedTouristicPlaces.Item3;
                    
                    touristicGoogleIds.Remove(selectedTouristicPlaces.Item1.GoogleId);
                    touristicGoogleIds.Remove(selectedTouristicPlaces.Item2.GoogleId);
                    touristicGoogleIds.Remove(selectedTouristicPlaces.Item3.GoogleId);
                    
                    standardRoute.TravelDays[day].PlaceAfterDinner = await SelectUniqueAfterDinnerPlace(
                        afterDinnerGoogleIds, allUsedGoogleIds, random, price_level, day, afterDinnerQuery);
                    
                    afterDinnerGoogleIds.Remove(standardRoute.TravelDays[day].PlaceAfterDinner.GoogleId);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Could not select places for day {Day}.", day);
                    await GetMoreUniquePlaces(destination, touristicGoogleIds, afterDinnerGoogleIds, allUsedGoogleIds, personalizationPrefs, price_level);
                    
                    day--; 
                }
            }
            
            bool isValid = RouteTestWithDetails(standardRoute);

            var response = await GetGeminiDataForRouteAsync(standardRoute, startDate, endDate);

            if(response != null) 
            {
                for (int i = 0; i < standardRoute.TravelDays.Count; i++)
                {
                    standardRoute.TravelDays[i].Accomodation.Star = response.star;
                    standardRoute.TravelDays[i].ApproxPrice = response.approxPrices?[i];
                    standardRoute.TravelDays[i].DayDescription = response.dayDescriptions?[i];
                    standardRoute.TravelDays[i].Breakfast.Weather = response.weathers?[i][0];
                    standardRoute.TravelDays[i].FirstPlace.Weather = response.weathers?[i][1];
                    standardRoute.TravelDays[i].Lunch.Weather = response.weathers?[i][2];
                    standardRoute.TravelDays[i].SecondPlace.Weather = response.weathers?[i][3];
                    standardRoute.TravelDays[i].ThirdPlace.Weather = response.weathers?[i][4];
                    standardRoute.TravelDays[i].Dinner.Weather = response.weathers?[i][5];
                    standardRoute.TravelDays[i].PlaceAfterDinner.Weather = response.weathers?[i][6];
                }

            }

            standardRoute.Id = Guid.NewGuid();
            await SaveRouteToDatabase(standardRoute);
            standardRoute.User = null;

            return standardRoute;
        }

        private bool RouteTestWithDetails(StandardRoute? route)
        {
            if (route?.TravelDays == null || !route.TravelDays.Any())
                return false;

            int totalPlacesExpected = route.TravelDays.Count * 7; // 7 places per day
            var googleIdMap = new Dictionary<string, List<string>>();

            foreach (var day in route.TravelDays)
            {
                if (day == null) continue;

                CheckAndAddToMap(day.Breakfast?.GoogleId, "Breakfast", googleIdMap);
                CheckAndAddToMap(day.Lunch?.GoogleId, "Lunch", googleIdMap);
                CheckAndAddToMap(day.Dinner?.GoogleId, "Dinner", googleIdMap);
                CheckAndAddToMap(day.FirstPlace?.GoogleId, "FirstPlace", googleIdMap);
                CheckAndAddToMap(day.SecondPlace?.GoogleId, "SecondPlace", googleIdMap);
                CheckAndAddToMap(day.ThirdPlace?.GoogleId, "ThirdPlace", googleIdMap);
                CheckAndAddToMap(day.PlaceAfterDinner?.GoogleId, "PlaceAfterDinner", googleIdMap);
            }

            bool hasDuplicates = false;
            foreach (var entry in googleIdMap)
            {
                if (entry.Value.Count > 1)
                {
                    hasDuplicates = true;
                    _logger.LogDebug("Duplicate place {GoogleId} is used in: {Fields}", entry.Key, string.Join(", ", entry.Value));
                }
            }

            int uniqueCount = googleIdMap.Count;
            if (uniqueCount < totalPlacesExpected)
            {
                _logger.LogWarning("Not enough unique places: found {Found}, expected {Expected}.", uniqueCount, totalPlacesExpected);
                return false;
            }

            return !hasDuplicates;
        }

        private void CheckAndAddToMap(string? googleId, string placeType, Dictionary<string, List<string>> googleIdMap)
        {
            if (string.IsNullOrEmpty(googleId)) return;

            if (!googleIdMap.ContainsKey(googleId))
            {
                googleIdMap[googleId] = new List<string>();
            }
            googleIdMap[googleId].Add(placeType);
        }

        private async Task EnsureEnoughUniquePlaces(
            string destination, 
            int dayCount, 
            List<string> touristicGoogleIds, 
            List<string> afterDinnerGoogleIds, 
            HashSet<string> usedGoogleIds)
        {
            if (touristicGoogleIds.Count < (dayCount * 3))
            {
                var extraTouristicQueries = new[]
                {
                    "Must visit attractions",
                    "Top attractions",
                    "Things to do in",
                    "Popular sites",
                    "Tourist sites",
                    "Landmarks in",
                    "Famous places in"
                };

                foreach (var query in extraTouristicQueries)
                {
                    if (touristicGoogleIds.Count >= (dayCount * 4)) break;

                    var places = await _googlePlacesService.SearchPlacesAsync($"{query} {destination}");
                    if (places?.Places != null)
                    {
                        places.Places = FilterUnwantedPlaceTypes(places.Places);
                        var newIds = places.Places
                            .Where(p => p?.Id != null)
                            .Select(p => p.Id)
                            .Where(id => !touristicGoogleIds.Contains(id) && !usedGoogleIds.Contains(id))
                            .ToList();

                        touristicGoogleIds.AddRange(newIds);
                    }
                }
            }

            if (afterDinnerGoogleIds.Count < dayCount)
            {
                var extraAfterDinnerQueries = new[]
                {
                    "Evening entertainment",
                    "Nightlife",
                    "Bars",
                    "Pubs",
                    "Cafes",
                    "Late night venues"
                };

                foreach (var query in extraAfterDinnerQueries)
                {
                    if (afterDinnerGoogleIds.Count >= (dayCount * 2)) break;

                    var places = await _googlePlacesService.SearchPlacesAsync($"{query} in {destination}");
                    if (places?.Places != null)
                    {
                        places.Places = FilterUnwantedPlaceTypes(places.Places);
                        var newIds = places.Places
                            .Where(p => p?.Id != null)
                            .Select(p => p.Id)
                            .Where(id => !afterDinnerGoogleIds.Contains(id) && !usedGoogleIds.Contains(id))
                            .ToList();

                        afterDinnerGoogleIds.AddRange(newIds);
                    }
                }
            }
        }

        private async Task GetMoreUniquePlaces(
            string destination, 
            List<string> touristicGoogleIds, 
            List<string> afterDinnerGoogleIds, 
            HashSet<string> usedGoogleIds,
            List<PreferenceItem> personalizationPrefs,
            PRICE_LEVEL? priceLevel)
        {
            var differentTouristicQueries = new[]
            {
                "Hidden gems in",
                "Off the beaten path in",
                "Unusual attractions in",
                "Secret spots in",
                "Lesser known attractions in",
                "Local favorites in"
            };

            var randomQuery = differentTouristicQueries[new Random().Next(differentTouristicQueries.Length)];
            var places = await _googlePlacesService.SearchPlacesAsync($"{randomQuery} {destination}");
            if (places?.Places != null)
            {
                places.Places = FilterUnwantedPlaceTypes(places.Places);
                var newIds = places.Places
                    .Where(p => p?.Id != null)
                    .Select(p => p.Id)
                    .Where(id => !touristicGoogleIds.Contains(id) && !usedGoogleIds.Contains(id))
                    .ToList();

                touristicGoogleIds.AddRange(newIds);
            }

            string differentAfterDinnerQuery = GetDifferentAfterDinnerQuery(personalizationPrefs, priceLevel);
            var afterDinnerPlaces = await _googlePlacesService.SearchPlacesAsync($"{differentAfterDinnerQuery} in {destination}");
            if (afterDinnerPlaces?.Places != null)
            {
                afterDinnerPlaces.Places = FilterUnwantedPlaceTypes(afterDinnerPlaces.Places);
                var newIds = afterDinnerPlaces.Places
                    .Where(p => p?.Id != null)
                    .Select(p => p.Id)
                    .Where(id => !afterDinnerGoogleIds.Contains(id) && !usedGoogleIds.Contains(id))
                    .ToList();

                afterDinnerGoogleIds.AddRange(newIds);
            }
        }
        
        private string GetDifferentAfterDinnerQuery(List<PreferenceItem> personalizationPrefs, PRICE_LEVEL? priceLevel)
        {
            string pricePrefix = GetPriceQueryPrefix(priceLevel);
            
            var alternativeQueries = new[]
            {
                "Quiet cafes",
                "Jazz bars",
                "Karaoke places",
                "Comedy clubs",
                "Dessert places",
                "Late night food",
                "Evening entertainment",
                "Classy lounges"
            };
            
            return $"{pricePrefix}{alternativeQueries[new Random().Next(alternativeQueries.Length)]}";
        }
        
        private async Task<(Domain.Entities.Route.Place, Domain.Entities.Route.Place, Domain.Entities.Route.Place)> 
            SelectThreeUniqueTouristicPlaces(
                List<string> touristicGoogleIds, 
                HashSet<string> allUsedGoogleIds,
                Random random,
                int dayIndex,
                string touristicQuery)
        {
            if (touristicGoogleIds.Count < 3)
            {
                throw new InvalidOperationException("Not enough unique touristic places available");
            }
            
            var shuffledIds = ShuffleWithSeed(touristicGoogleIds, dayIndex);
            
            var firstPlaceId = shuffledIds[0];
            var secondPlaceId = shuffledIds[1];
            var thirdPlaceId = shuffledIds[2];
            
            var firstPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(firstPlaceId);
            var firstPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(firstPlaceDetails);
            firstPlace.Id = Guid.NewGuid();
            firstPlace.GoogleId = firstPlaceId;
            firstPlace.UserPersonalizationPref = touristicQuery; // Actual preference value used
            allUsedGoogleIds.Add(firstPlaceId);
            
            var secondPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(secondPlaceId);
            var secondPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(secondPlaceDetails);
            secondPlace.Id = Guid.NewGuid();
            secondPlace.GoogleId = secondPlaceId;
            secondPlace.UserPersonalizationPref = touristicQuery; // Actual preference value used
            allUsedGoogleIds.Add(secondPlaceId);
            
            var thirdPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(thirdPlaceId);
            var thirdPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(thirdPlaceDetails);
            thirdPlace.Id = Guid.NewGuid();
            thirdPlace.GoogleId = thirdPlaceId;
            thirdPlace.UserPersonalizationPref = touristicQuery; // Actual preference value used
            allUsedGoogleIds.Add(thirdPlaceId);
            
            return (firstPlace, secondPlace, thirdPlace);
        }
        
        private async Task<PlaceAfterDinner> SelectUniqueAfterDinnerPlace(
            List<string> afterDinnerGoogleIds,
            HashSet<string> allUsedGoogleIds,
            Random random,
            PRICE_LEVEL? priceLevel,
            int dayIndex,
            string afterDinnerQuery)
        {
            if (afterDinnerGoogleIds.Count < 1)
            {
                throw new InvalidOperationException("Not enough unique after dinner places available");
            }
            
            var shuffledIds = ShuffleWithSeed(afterDinnerGoogleIds, dayIndex);
            
            var placeId = shuffledIds[0];
            
            var placeDetails = await _googlePlacesService.GetPlaceDetailsAsync(placeId);
            var place = JsonConvert.DeserializeObject<PlaceAfterDinner>(placeDetails);
            
            place.Id = Guid.NewGuid();
            place._PRICE_LEVEL = priceLevel;
            place.GoogleId = placeId;
            place.UserFoodPreference = afterDinnerQuery; // Actual preference value used
            allUsedGoogleIds.Add(placeId);
            
            return place;
        }
        
        private List<T> ShuffleWithSeed<T>(List<T> list, int seed)
        {
            var random = new Random(seed);
            var result = new List<T>(list);
            
            int n = result.Count;
            while (n > 1)
            {
                n--;
                int k = random.Next(n + 1);
                T value = result[k];
                result[k] = result[n];
                result[n] = value;
            }
            
            return result;
        }

        private async Task<T> FindAndSelectMealWithPreferences<T>(
            string destination, 
            PRICE_LEVEL? priceLevel, 
            string foodQueryPrefix, 
            Random random,
            int dayIndex,
            HashSet<string> allUsedGoogleIds) where T : class, new()
        {
            string mealQuery = $"{foodQueryPrefix} in {destination}";
            
            var mealList = await _googlePlacesService.SearchPlacesAsync(mealQuery);
            
            if (mealList?.Places == null || mealList.Places.Count == 0)
            {
                MealType mealType = typeof(T).Name switch
                {
                    nameof(Breakfast) => MealType.Breakfast,
                    nameof(Lunch) => MealType.Lunch,
                    nameof(Dinner) => MealType.Dinner,
                    _ => MealType.Breakfast
                };

                string simplifiedQuery = mealType switch
                {
                    MealType.Breakfast => $"{GetPriceQueryPrefix(priceLevel)}{BREAKFAST_QUERY} in {destination}",
                    MealType.Lunch => $"{GetPriceQueryPrefix(priceLevel)}{LUNCH_QUERY} in {destination}",
                    MealType.Dinner => $"{GetPriceQueryPrefix(priceLevel)}{DINNER_QUERY} in {destination}",
                    _ => $"restaurant in {destination}"
                };
                
                mealList = await _googlePlacesService.SearchPlacesAsync(simplifiedQuery);
            }
            
            if (mealList?.Places == null || mealList.Places.Count == 0)
            {
                mealList = await _googlePlacesService.SearchPlacesAsync($"restaurant in {destination}");
            }
            
            if (mealList?.Places != null)
                mealList.Places = FilterUnwantedPlaceTypes(mealList.Places);
            
            if (mealList?.Places == null || mealList.Places.Count == 0)
            {
                throw new InvalidOperationException($"No suitable meal places found in {destination}");
            }
            
            var uniquePlaces = mealList.Places
                .Where(p => p?.Id != null && !allUsedGoogleIds.Contains(p.Id))
                .ToList();
            
            if (uniquePlaces.Count == 0)
            {
                MealType mealType = typeof(T).Name switch
                {
                    nameof(Breakfast) => MealType.Breakfast,
                    nameof(Lunch) => MealType.Lunch,
                    nameof(Dinner) => MealType.Dinner,
                    _ => MealType.Breakfast
                };
                
                string alternativeQuery = GetAlternativeFoodQuery(mealType, destination, priceLevel);
                var alternateMealList = await _googlePlacesService.SearchPlacesAsync(alternativeQuery);
                
                if (alternateMealList?.Places != null)
                {
                    alternateMealList.Places = FilterUnwantedPlaceTypes(alternateMealList.Places);
                    uniquePlaces = alternateMealList.Places
                        .Where(p => p?.Id != null && !allUsedGoogleIds.Contains(p.Id))
                        .ToList();
                }
                
                if (uniquePlaces.Count == 0)
                {
                    uniquePlaces = mealList.Places
                        .Where(p => p?.Id != null)
                        .ToList();
                    
                    _logger.LogWarning("Could not find unique {PlaceType} places; some places may be reused.", typeof(T).Name);
                }
            }
            
            int indexOffset = dayIndex % Math.Max(1, uniquePlaces.Count);
            int selectIndex = (random.Next(uniquePlaces.Count) + indexOffset) % uniquePlaces.Count;
            
            var selectedPlace = uniquePlaces[selectIndex];
            
            var placeDetails = await _googlePlacesService.GetPlaceDetailsAsync(selectedPlace.Id);
            T mealPlace = JsonConvert.DeserializeObject<T>(placeDetails);
            
            typeof(T).GetProperty("Id")?.SetValue(mealPlace, Guid.NewGuid());
            typeof(T).GetProperty("_PRICE_LEVEL")?.SetValue(mealPlace, priceLevel);
            typeof(T).GetProperty("GoogleId")?.SetValue(mealPlace, selectedPlace.Id);
            
            typeof(T).GetProperty("UserFoodPreference")?.SetValue(mealPlace, foodQueryPrefix);
            
            allUsedGoogleIds.Add(selectedPlace.Id);
            
            return mealPlace;
        }
        
        private string GetAlternativeFoodQuery(MealType mealType, string destination, PRICE_LEVEL? priceLevel)
        {
            string pricePrefix = GetPriceQueryPrefix(priceLevel);
            
            switch (mealType)
            {
                case MealType.Breakfast:
                    var breakfastQueries = new[] { 
                        "Cafe", 
                        "Coffee shop", 
                        "Bakery", 
                        "Brunch", 
                        "Morning restaurant" 
                    };
                    return $"{pricePrefix}{breakfastQueries[new Random().Next(breakfastQueries.Length)]} in {destination}";
                    
                case MealType.Lunch:
                    var lunchQueries = new[] { 
                        "Bistro", 
                        "Casual restaurant", 
                        "Deli", 
                        "Sandwich shop", 
                        "Quick lunch" 
                    };
                    return $"{pricePrefix}{lunchQueries[new Random().Next(lunchQueries.Length)]} in {destination}";
                    
                case MealType.Dinner:
                    var dinnerQueries = new[] { 
                        "Fine dining", 
                        "Restaurant", 
                        "Steakhouse", 
                        "Grill", 
                        "Evening dining" 
                    };
                    return $"{pricePrefix}{dinnerQueries[new Random().Next(dinnerQueries.Length)]} in {destination}";
                    
                default:
                    return $"{pricePrefix}Restaurant in {destination}";
            }
        }

        private string GetTouristicPlacesQueryPrefix(List<PreferenceItem> personalizationPrefs)
        {
            if (personalizationPrefs == null || personalizationPrefs.Count < 2)
                return "Tourist attractions";
            
            string selectedPreference = SelectPreferenceWithWeightedRandom(personalizationPrefs);
            
            switch (selectedPreference)
            {
                case "AdventurePreference":
                    return "Adventure activities";
                case "RelaxationPreference":
                    return "Relaxation spots";
                case "CulturalPreference":
                    return "Cultural attractions";
                case "NaturePreference":
                    return "Breathtaking natural attractions";
                case "UrbanPreference":
                    return "Urban attractions";
                case "RuralPreference":
                    return "Rural attractions";
                case "LuxuryPreference":
                    return "Exclusive attractions";
                case "BudgetPreference":
                    return "Free tourist attractions";
                case "SoloTravelPreference":
                    return "Solo traveler spots";
                case "GroupTravelPreference":
                    return "Group friendly attractions";
                case "FamilyTravelPreference":
                    return "Family-friendly attractions";
                case "CoupleTravelPreference":
                    return "Romantic spots";
                case "BeachPreference":
                    return "Best beaches";
                case "MountainPreference":
                    return "Mountain attractions";
                case "DesertPreference":
                    return "Desert attractions";
                case "ForestPreference":
                    return "Forest attractions";
                case "IslandPreference":
                    return "Island attractions";
                case "LakePreference":
                    return "Lake attractions";
                case "RiverPreference":
                    return "River attractions";
                case "WaterfallPreference":
                    return "Waterfall attractions";
                case "CavePreference":
                    return "Cave attractions";
                case "VolcanoPreference":
                    return "Volcano attractions";
                case "GlacierPreference":
                    return "Glacier attractions";
                case "CanyonPreference":
                    return "Canyon attractions";
                case "ValleyPreference":
                    return "Valley attractions";
                default:
                    return "Famous tourist attractions";
            }
        }

        private string GetAdditionalTouristicQuery(List<PreferenceItem> personalizationPrefs, string primaryQuery)
        {
            var defaultQueries = new List<string>
            {
                "Must visit spots",
                "Popular tourist destinations",
                "Top-rated places to visit",
                "Interesting places to see"
            };
            
            if (personalizationPrefs == null || personalizationPrefs.Count < 2)
                return defaultQueries[new Random().Next(defaultQueries.Count)];
            
            var filteredPrefs = personalizationPrefs
                .Where(p => !primaryQuery.Contains(ConvertPreferenceNameToKeyword(p.Name)))
                .ToList();
            
            if (filteredPrefs.Count == 0)
                return defaultQueries[new Random().Next(defaultQueries.Count)];
                
            string selectedPreference = SelectPreferenceWithWeightedRandom(filteredPrefs);
            
            switch (selectedPreference)
            {
                case "AdventurePreference":
                    return "Adventure spots";
                case "RelaxationPreference":
                    return "Relaxation sites";
                case "CulturalPreference":
                    return "Cultural sites";
                case "NaturePreference":
                    return "Natural attractions";
                case "UrbanPreference":
                    return "Urban attractions";
                case "RuralPreference":
                    return "Rural attractions";
                case "LuxuryPreference":
                    return "Luxury attractions";
                case "BudgetPreference":
                    return "Budget attractions";
                case "SoloTravelPreference":
                    return "Solo traveler spots";
                case "GroupTravelPreference":
                    return "Group attractions";
                case "FamilyTravelPreference":
                    return "Family attractions";
                case "CoupleTravelPreference":
                    return "Romantic sites";
                case "BeachPreference":
                    return "Beach attractions";
                case "MountainPreference":
                    return "Mountain attractions";
                case "DesertPreference":
                    return "Desert attractions";
                case "ForestPreference":
                    return "Forest attractions";
                case "IslandPreference":
                    return "Island attractions";
                case "LakePreference":
                    return "Lake attractions";
                case "RiverPreference":
                    return "River attractions";
                case "WaterfallPreference":
                    return "Waterfall sites";
                case "CavePreference":
                    return "Cave attractions";
                case "VolcanoPreference":
                    return "Volcano sites";
                case "GlacierPreference":
                    return "Glacier attractions";
                case "CanyonPreference":
                    return "Canyon attractions";
                case "ValleyPreference":
                    return "Valley attractions";
                default:
                    return defaultQueries[new Random().Next(defaultQueries.Count)];
            }
        }
        
        private string GetAfterDinnerQueryPrefix(List<PreferenceItem> personalizationPrefs, PRICE_LEVEL? priceLevel)
        {
            string pricePrefix = GetPriceQueryPrefix(priceLevel);
            
            var afterDinnerQueries = new[] { 
                "Live music bars",
                "Modern rooftop bars",
                "Famous cocktail bars", 
                "Special dessert shops",
                "Trendy nightclubs"
            };
            
            if (personalizationPrefs == null || personalizationPrefs.Count == 0)
                return $"{pricePrefix}{afterDinnerQueries[new Random().Next(afterDinnerQueries.Length)]}";
            
            string selectedPreference = SelectPreferenceWithWeightedRandom(personalizationPrefs);
            
            switch (selectedPreference)
            {
                case "AdventurePreference":
                    return $"{pricePrefix}Adventure activities";
                case "RelaxationPreference":
                    return $"{pricePrefix}Quiet lounge bars";
                case "CulturalPreference":
                    return $"{pricePrefix}Cultural evening activities";
                case "NaturePreference":
                    return $"{pricePrefix}Nature viewpoints";
                case "UrbanPreference":
                    return $"{pricePrefix}Urban nightlife";
                case "RuralPreference":
                    return $"{pricePrefix}Rural evening entertainment";
                case "LuxuryPreference":
                    return $"{pricePrefix}Exclusive rooftop bars";
                case "BudgetPreference":
                    return $"{pricePrefix}Budget friendly evening spots";
                case "SoloTravelPreference":
                    return $"{pricePrefix}Solo traveler friendly bars";
                case "GroupTravelPreference":
                    return $"{pricePrefix}Group friendly evening venues";
                case "FamilyTravelPreference":
                    return $"{pricePrefix}Family-friendly evening activities";
                case "CoupleTravelPreference":
                    return $"{pricePrefix}Romantic evening spots";
                case "BeachPreference":
                    return $"{pricePrefix}Beach bars";
                case "MountainPreference":
                    return $"{pricePrefix}Mountain viewpoint bars";
                case "IslandPreference":
                    return $"{pricePrefix}Island nightlife";
                default:
                    return $"{pricePrefix}{afterDinnerQueries[new Random().Next(afterDinnerQueries.Length)]}";
            }
        }
        
        private async Task<List<string>> FindTouristicPlacesWithPreferences(string destination, string touristicQuery, int requiredCount)
        {
            var allTouristicPlaces = new List<PlaceSearchResponse>();
            
            var places = await _googlePlacesService.SearchPlacesAsync($"{touristicQuery} in {destination}");
            if (places?.Places != null)
            {
                places.Places = FilterUnwantedPlaceTypes(places.Places);
                allTouristicPlaces.Add(places);
            }
            
            if (allTouristicPlaces.SelectMany(p => p.Places).Count() < requiredCount)
            {
                var additionalQueries = new[] {
                    "Tourist attractions",
                    "Must visit spots",
                    "Top rated attractions"
                };
                
                foreach (var query in additionalQueries)
                {
                    if (allTouristicPlaces.SelectMany(p => p.Places).Count() >= requiredCount)
                        break;
                        
                    var additionalPlaces = await _googlePlacesService.SearchPlacesAsync($"{query} in {destination}");
                    if (additionalPlaces?.Places != null)
                    {
                        additionalPlaces.Places = FilterUnwantedPlaceTypes(additionalPlaces.Places);
                        allTouristicPlaces.Add(additionalPlaces);
                    }
                }
            }
            
            return allTouristicPlaces
                .SelectMany(p => p.Places ?? new List<Application.DTOs.GooglePlaces.Place>())
                .Where(p => !string.IsNullOrEmpty(p?.Id))
                .Select(p => p!.Id!)
                .Distinct()
                .ToList();
        }
        
        private async Task<List<string>> FindMoreTouristicPlaces(string destination, int requiredCount, List<string> existingIds, string additionalQuery)
        {
            var additionalIds = new List<string>();
            
            var places = await _googlePlacesService.SearchPlacesAsync($"{additionalQuery} in {destination}");
            if (places?.Places != null)
            {
                places.Places = FilterUnwantedPlaceTypes(places.Places);
                
                var newIds = places.Places
                    .Where(p => p?.Id != null)
                    .Select(p => p.Id)
                    .Where(id => !existingIds.Contains(id) && !additionalIds.Contains(id))
                    .ToList();
                    
                additionalIds.AddRange(newIds);
            }
            
            if (existingIds.Count + additionalIds.Count < requiredCount)
            {
                var genericQueries = new[] {
                    "Sightseeing",
                    "Famous landmarks",
                    "Points of interest",
                    "Places to visit"
                };
                
                foreach (var query in genericQueries)
                {
                    if (existingIds.Count + additionalIds.Count >= requiredCount)
                        break;
                        
                    places = await _googlePlacesService.SearchPlacesAsync($"{query} in {destination}");
                    if (places?.Places != null)
                    {
                        places.Places = FilterUnwantedPlaceTypes(places.Places);
                        
                        var newIds = places.Places
                            .Where(p => p?.Id != null)
                            .Select(p => p.Id)
                            .Where(id => !existingIds.Contains(id) && !additionalIds.Contains(id))
                            .ToList();
                            
                        additionalIds.AddRange(newIds);
                    }
                }
            }
            
            return additionalIds;
        }
        
        private async Task<List<string>> FindAfterDinnerPlacesWithPreferences(
            string destination, 
            string afterDinnerQuery, 
            PRICE_LEVEL? priceLevel)
        {
            var afterDinnerGoogleIds = new List<string>();
            
            var places = await _googlePlacesService.SearchPlacesAsync($"{afterDinnerQuery} in {destination}");
            if (places?.Places != null)
            {
                places.Places = FilterUnwantedPlaceTypes(places.Places);
                var validIds = places.Places
                    .Where(p => !string.IsNullOrEmpty(p?.Id))
                    .Select(p => p!.Id!)
                    .ToList();
                afterDinnerGoogleIds.AddRange(validIds);
            }
            
            if (afterDinnerGoogleIds.Count < 5)
            {
                var additionalQueries = new[]
                {
                    $"{GetPriceQueryPrefix(priceLevel)} Evening entertainment",
                    $"{GetPriceQueryPrefix(priceLevel)} Night activities",
                    "Popular evening spots"
                };
                
                foreach (var query in additionalQueries)
                {
                    places = await _googlePlacesService.SearchPlacesAsync($"{query} in {destination}");
                    if (places?.Places != null)
                    {
                        places.Places = FilterUnwantedPlaceTypes(places.Places);
                        var newIds = places.Places
                            .Where(p => !string.IsNullOrEmpty(p?.Id))
                            .Select(p => p!.Id!)
                            .Where(id => !afterDinnerGoogleIds.Contains(id))
                            .ToList();
                            
                        afterDinnerGoogleIds.AddRange(newIds);
                    }
                    
                    if (afterDinnerGoogleIds.Count >= 10)
                        break;
                }
            }
            
            return afterDinnerGoogleIds;
        }

        private async Task<(Domain.Entities.Route.Place firstPlace, Domain.Entities.Route.Place secondPlace, Domain.Entities.Route.Place thirdPlace)>
            SelectThreeTouristicPlacesWithPreferences(
                List<string> touristicGoogleIds, 
                HashSet<int> usedIndices, 
                HashSet<string> usedGoogleIds, 
                Random random,
                int dayIndex)
        {
            int offset = dayIndex * 3 % Math.Max(1, touristicGoogleIds.Count);
            
            int firstPlaceIndex = await GetUniqueIndexWithOffset(touristicGoogleIds, usedIndices, usedGoogleIds, random, offset);
            var firstPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[firstPlaceIndex]);
            var firstPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(firstPlaceDetails);
            firstPlace.Id = Guid.NewGuid();
            firstPlace.GoogleId = touristicGoogleIds[firstPlaceIndex];

            int secondPlaceIndex = await GetUniqueIndexWithOffset(touristicGoogleIds, usedIndices, usedGoogleIds, random, offset + 1);
            var secondPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[secondPlaceIndex]);
            var secondPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(secondPlaceDetails);
            secondPlace.Id = Guid.NewGuid();
            secondPlace.GoogleId = touristicGoogleIds[secondPlaceIndex];

            int thirdPlaceIndex = await GetUniqueIndexWithOffset(touristicGoogleIds, usedIndices, usedGoogleIds, random, offset + 2);
            var thirdPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[thirdPlaceIndex]);
            var thirdPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(thirdPlaceDetails);
            thirdPlace.Id = Guid.NewGuid();
            thirdPlace.GoogleId = touristicGoogleIds[thirdPlaceIndex];

            return (firstPlace, secondPlace, thirdPlace);
        }

        private async Task<int> GetUniqueIndexWithOffset(
            List<string> googleIds, 
            HashSet<int> usedIndices, 
            HashSet<string> usedGoogleIds, 
            Random random,
            int offset)
        {
            var availableIndices = new List<int>();
            
            for (int i = 0; i < googleIds.Count; i++)
            {
                if (!usedIndices.Contains(i) && !usedGoogleIds.Contains(googleIds[i]))
                {
                    availableIndices.Add(i);
                }
            }
            
            int index;
            
            if (availableIndices.Count > 0)
            {
                int offsetIndex = (offset % availableIndices.Count);
                index = availableIndices[offsetIndex];
            }
            else
            {
                for (int i = 0; i < googleIds.Count; i++)
                {
                    if (!usedGoogleIds.Contains(googleIds[i]))
                    {
                        index = i;
                        usedIndices.Add(index);
                        usedGoogleIds.Add(googleIds[index]);
                        return index;
                    }
                }
                
                index = random.Next(googleIds.Count);
            }
            
            usedIndices.Add(index);
            usedGoogleIds.Add(googleIds[index]);
            
            return index;
        }

        private async Task<PlaceAfterDinner> SelectAfterDinnerPlaceWithPreferences(
            List<string> afterDinnerGoogleIds,
            HashSet<int> usedIndices,
            HashSet<string> usedGoogleIds,
            Random random,
            PRICE_LEVEL? priceLevel,
            int dayIndex)
        {
            int offset = dayIndex % Math.Max(1, afterDinnerGoogleIds.Count);
            
            int index = await GetUniqueIndexWithOffset(afterDinnerGoogleIds, usedIndices, usedGoogleIds, random, offset);
            
            var placeDetails = await _googlePlacesService.GetPlaceDetailsAsync(afterDinnerGoogleIds[index]);
            var place = JsonConvert.DeserializeObject<PlaceAfterDinner>(placeDetails);
            
            place.Id = Guid.NewGuid();
            place._PRICE_LEVEL = priceLevel;
            place.GoogleId = afterDinnerGoogleIds[index];
            
            return place;
        }

        private async Task<bool> IsFriendsAsync(string userId1, string userId2)
        {
            return await _context.UserFriendships
                .AnyAsync(f => 
                    ((f.RequesterId == userId1 && f.AddresseeId == userId2) || 
                     (f.RequesterId == userId2 && f.AddresseeId == userId1)) && 
                    f.Status == Domain.Enums.FriendshipStatus.Accepted);
        }
        
        private async Task<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)> GetUserPreferencesAsync(AppUser user)
        {
            var accommodationPrefs = await _context.Set<UserAccommodationPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
                
            var foodPrefs = await _context.Set<UserFoodPreferences>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
                
            var personalizationPrefs = await _context.Set<UserPersonalization>()
                .FirstOrDefaultAsync(p => p.UserId == user.Id);
                
            return (accommodationPrefs, foodPrefs, personalizationPrefs);
        }

        private void ValidateParameters(string? destination, DateOnly? startDate, DateOnly? endDate, string? username)
        {
            if (destination == null || startDate == null || endDate == null || username == null)
                throw new ArgumentNullException();
        }

        private List<PreferenceItem> ShuffleWithWeights(List<PreferenceItem> preferences)
        {
            var random = new Random();
            var result = new List<PreferenceItem>();
            
            var workingCopy = new List<PreferenceItem>(preferences);
            
            while (workingCopy.Count > 0)
            {
                int totalWeight = workingCopy.Sum(p => p.Value);
                
                if (totalWeight == 0)
                {
                    int index = random.Next(workingCopy.Count);
                    result.Add(workingCopy[index]);
                    workingCopy.RemoveAt(index);
                    continue;
                }
                
                int randomValue = random.Next(totalWeight + 1);
                int cumulativeWeight = 0;
                
                for (int i = 0; i < workingCopy.Count; i++)
                {
                    cumulativeWeight += workingCopy[i].Value;
                    if (randomValue <= cumulativeWeight)
                    {
                        result.Add(workingCopy[i]);
                        workingCopy.RemoveAt(i);
                        break;
                    }
                }
            }
            
            return result;
        }
        
        private List<string> SelectMultiplePreferencesWithWeightedRandom(List<PreferenceItem> preferences, int count)
        {
            var selectedPreferences = new List<string>();
            
            if (preferences == null || preferences.Count == 0)
                return selectedPreferences;
            
            var mandatoryPreferences = preferences.Where(p => p.IsMandatory).ToList();
            foreach (var pref in mandatoryPreferences.OrderByDescending(p => p.Value))
            {
                selectedPreferences.Add(pref.Name);
            }
            
            var dominantPreferences = preferences.Where(p => p.IsDominant && !p.IsMandatory && p.Value > 60).ToList();
            foreach (var pref in dominantPreferences.OrderByDescending(p => p.Value))
            {
                if (selectedPreferences.Count < count && !selectedPreferences.Contains(pref.Name))
                {
                    selectedPreferences.Add(pref.Name);
                }
            }
            
            var nonDominantPreferences = preferences.Where(p => !p.IsDominant && !p.IsMandatory).ToList();
            nonDominantPreferences = nonDominantPreferences.OrderByDescending(p => p.Value).ToList();
            
            while (selectedPreferences.Count < count && nonDominantPreferences.Count > 0)
            {
                int totalWeight = nonDominantPreferences.Sum(p => p.Value);
                if (totalWeight == 0) 
                {
                    if (!selectedPreferences.Contains(nonDominantPreferences[0].Name))
                    {
                        selectedPreferences.Add(nonDominantPreferences[0].Name);
                    }
                    nonDominantPreferences.RemoveAt(0);
                    continue;
                }
                
                var random = new Random();
                int randomValue = random.Next(0, totalWeight + 1);
                
                int cumulativeWeight = 0;
                for (int i = 0; i < nonDominantPreferences.Count; i++)
                {
                    cumulativeWeight += nonDominantPreferences[i].Value;
                    if (randomValue <= cumulativeWeight)
                    {
                        if (!selectedPreferences.Contains(nonDominantPreferences[i].Name))
                        {
                            selectedPreferences.Add(nonDominantPreferences[i].Name);
                        }
                        nonDominantPreferences.RemoveAt(i);
                        break;
                    }
                }
                
                if (nonDominantPreferences.Count == 0 && selectedPreferences.Count < count)
                {
                    nonDominantPreferences = preferences.Where(p => !p.IsMandatory || !selectedPreferences.Contains(p.Name))
                                                      .OrderByDescending(p => p.Value)
                                                      .ToList();
                    
                    nonDominantPreferences = ShuffleWithWeights(nonDominantPreferences);
                }
            }
            
            while (selectedPreferences.Count > count)
            {
                var nonMandatoryPrefs = selectedPreferences
                    .Where(name => !preferences.Any(p => p.Name == name && p.IsMandatory))
                    .ToList();
                
                if (nonMandatoryPrefs.Count > 0)
                {
                    var lowestValuePref = nonMandatoryPrefs
                        .OrderBy(name => preferences.FirstOrDefault(p => p.Name == name)?.Value ?? 0)
                        .First();
                    
                    selectedPreferences.Remove(lowestValuePref);
                }
                else
                {
                    break;
                }
            }
            
            return selectedPreferences;
        }
        
        private string SelectPreferenceWithWeightedRandom(List<PreferenceItem> preferences)
        {
            if (preferences == null || preferences.Count == 0)
                return string.Empty;
                
            var mandatoryPreferences = preferences.Where(p => p.IsMandatory).ToList();
            if (mandatoryPreferences.Count > 0)
            {
                return mandatoryPreferences.OrderByDescending(p => p.Value).First().Name;
            }
            
            preferences = preferences.OrderByDescending(p => p.Value).ToList();
            
            var dominantPreferences = preferences.Where(p => p.IsDominant && p.Value > 60).ToList();
            if (dominantPreferences.Count > 0)
            {
                if (new Random().Next(100) < 80)
                {
                    int totalWeight = dominantPreferences.Sum(p => p.Value);
                    
                    if (totalWeight == 0)
                        return dominantPreferences[0].Name;
                        
                    var random = new Random();
                    int randomValue = random.Next(0, totalWeight + 1);
                    
                    int cumulativeWeight = 0;
                    foreach (var preference in dominantPreferences)
                    {
                        cumulativeWeight += preference.Value;
                        if (randomValue <= cumulativeWeight)
                        {
                            return preference.Name;
                        }
                    }
                    
                    return dominantPreferences[0].Name;
                }
            }
            
            int totalWeightAll = preferences.Sum(p => p.Value);
            
            if (totalWeightAll == 0)
                return preferences[0].Name;
                
            var randomAll = new Random();
            int randomValueAll = randomAll.Next(0, totalWeightAll + 1);
            
            int cumulativeWeightAll = 0;
            foreach (var preference in preferences)
            {
                cumulativeWeightAll += preference.Value;
                if (randomValueAll <= cumulativeWeightAll)
                {
                    return preference.Name;
                }
            }
            
            return preferences[0].Name;
        }
        
        private class PreferenceItem
        {
            public string Name { get; set; }
            public int Value { get; set; }
            public bool IsDominant { get; set; } = false;
            public bool IsMandatory { get; set; } = false; 
        }

        private string GetSelectedAccommodationPreference(List<PreferenceItem> accommodationPrefs)
        {
            if (accommodationPrefs == null || accommodationPrefs.Count == 0)
            {
                return "No specific preference";
            }
            
            string selectedPreference = SelectPreferenceWithWeightedRandom(accommodationPrefs);
            
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

        private string GetAccommodationQueryPrefix(List<PreferenceItem> accommodationPrefs, PRICE_LEVEL? priceLevel)
        {
            string starRating = GetHotelQueryPrefix(priceLevel);
            
            string query = $"{starRating} star hotel";
            
            if (accommodationPrefs == null || accommodationPrefs.Count == 0)
            {
                return query;
            }
            
            foreach (var pref in accommodationPrefs.OrderByDescending(p => p.Value).Take(5))
            {
            }
            
            string selectedPreference = SelectPreferenceWithWeightedRandom(accommodationPrefs);
            
            var preferencesStringBuilder = new StringBuilder();
            
            switch (selectedPreference)
            {
                case "LuxuryHotelPreference":
                    preferencesStringBuilder.Append(" luxury");
                    break;
                case "BudgetHotelPreference":
                    preferencesStringBuilder.Append(" budget");
                    break;
                case "BoutiqueHotelPreference":
                    preferencesStringBuilder.Append(" boutique");
                    break;
                case "HostelPreference":
                    preferencesStringBuilder.Append(" hostel");
                    break;
                case "ApartmentPreference":
                    preferencesStringBuilder.Append(" apartment");
                    break;
                case "ResortPreference":
                    preferencesStringBuilder.Append(" resort");
                    break;
                case "VillaPreference":
                    preferencesStringBuilder.Append(" villa");
                    break;
                case "GuestHousePreference":
                    preferencesStringBuilder.Append(" guest house");
                    break;
                case "CampingPreference":
                    preferencesStringBuilder.Append(" camping");
                    break;
                case "GlampingPreference":
                    preferencesStringBuilder.Append(" glamping");
                    break;
                case "BedAndBreakfastPreference":
                    preferencesStringBuilder.Append(" bed and breakfast");
                    break;
                case "AllInclusivePreference":
                    preferencesStringBuilder.Append(" all inclusive");
                    break;
                case "SpaAndWellnessPreference":
                    preferencesStringBuilder.Append(" with spa");
                    break;
                case "PetFriendlyPreference":
                    preferencesStringBuilder.Append(" pet friendly");
                    break;
                case "EcoFriendlyPreference":
                    preferencesStringBuilder.Append(" eco friendly");
                    break;
                case "RemoteLocationPreference":
                    preferencesStringBuilder.Append(" remote location");
                    break;
                case "CityCenterPreference":
                    preferencesStringBuilder.Append(" city center");
                    break;
                case "FamilyFriendlyPreference":
                    preferencesStringBuilder.Append(" family friendly");
                    break;
                case "AdultsOnlyPreference":
                    preferencesStringBuilder.Append(" adults only");
                    break;
                case "HomestayPreference":
                    preferencesStringBuilder.Append(" homestay");
                    break;
                case "WaterfrontPreference":
                    preferencesStringBuilder.Append(" waterfront");
                    break;
                case "HistoricalBuildingPreference":
                    preferencesStringBuilder.Append(" historical building");
                    break;
                case "AirbnbPreference":
                    preferencesStringBuilder.Append(" airbnb style");
                    break;
                case "CoLivingSpacePreference":
                    preferencesStringBuilder.Append(" co-living space");
                    break;
                case "ExtendedStayPreference":
                    preferencesStringBuilder.Append(" extended stay");
                    break;
            }
            
            string finalQuery = query + preferencesStringBuilder.ToString();
            return finalQuery;
        }

        private async Task<TravelAccomodation> FindAndSelectHotelWithPreferences(
            string destination, 
            PRICE_LEVEL? priceLevel, 
            string accommodationQueryPrefix, 
            Random random,
            string selectedAccommodationPreference)
        {
            string hotelQuery = $"{accommodationQueryPrefix} in {destination}";
            var hotelList = await _googlePlacesService.SearchPlacesAsync(hotelQuery);
            
            if (hotelList?.Places == null || hotelList.Places.Count == 0)
            {
                string standardQuery = $"{GetHotelQueryPrefix(priceLevel)} star hotel in {destination}";
                hotelList = await _googlePlacesService.SearchPlacesAsync(standardQuery);
            }
            
            if (hotelList?.Places == null || hotelList.Places.Count == 0)
            {
                hotelList = await _googlePlacesService.SearchPlacesAsync($"hotel in {destination}");
            }
            
            if (hotelList?.Places != null)
            {
                int originalCount = hotelList.Places.Count;
                hotelList.Places = FilterUnwantedPlaceTypes(hotelList.Places);
            }
            
            if (hotelList?.Places == null || hotelList.Places.Count == 0)
            {
                throw new InvalidOperationException($"No suitable hotels found in {destination}");
            }
            
            int randomIndex = random.Next(hotelList.Places.Count);
            var placeId = hotelList.Places[randomIndex].Id;
            
            var placeDetails = await _googlePlacesService.GetPlaceDetailsAsync(placeId);
            var hotel = JsonConvert.DeserializeObject<TravelAccomodation>(placeDetails);
            hotel.Id = Guid.NewGuid();
            hotel._PRICE_LEVEL = priceLevel;
            hotel.GoogleId = placeId;
            
            hotel.UserAccomodationPreference = selectedAccommodationPreference;
            
            return hotel;
        }

        private enum MealType
        {
            Breakfast,
            Lunch,
            Dinner,
            AfterDinner
        }

        private string GetFoodQueryPrefix(List<PreferenceItem> foodPrefs, PRICE_LEVEL? priceLevel, MealType mealType)
        {
            string pricePrefix = GetPriceQueryPrefix(priceLevel);
            
            string mealPrefix = mealType switch
            {
                MealType.Breakfast => BREAKFAST_QUERY,
                MealType.Lunch => LUNCH_QUERY,
                MealType.Dinner => DINNER_QUERY,
                _ => BREAKFAST_QUERY
            };
            
            string baseQuery = $"{pricePrefix}{mealPrefix}";
            
            if (foodPrefs == null || foodPrefs.Count == 0)
                return baseQuery;
            
            string selectedPreference = SelectPreferenceWithWeightedRandom(foodPrefs);
            
            var queryBuilder = new StringBuilder(baseQuery);
            
            switch (selectedPreference)
            {
                case "VeganPreference":
                    queryBuilder.Append("vegan ");
                    break;
                case "VegetarianPreference":
                    queryBuilder.Append("vegetarian ");
                    break;
                case "GlutenFreePreference":
                    queryBuilder.Append("gluten free ");
                    break;
                case "HalalPreference":
                    queryBuilder.Append("halal ");
                    break;
                case "KosherPreference":
                    queryBuilder.Append("kosher ");
                    break;
                case "SeafoodPreference":
                    queryBuilder.Append("seafood ");
                    break;
                case "LocalCuisinePreference":
                    queryBuilder.Append("local cuisine ");
                    break;
                case "FastFoodPreference":
                    queryBuilder.Append("fast food ");
                    break;
                case "FinePreference":
                    queryBuilder.Append("fine dining ");
                    break;
                case "StreetFoodPreference":
                    queryBuilder.Append("street food ");
                    break;
                case "OrganicPreference":
                    queryBuilder.Append("organic ");
                    break;
                case "BuffetPreference":
                    queryBuilder.Append("buffet ");
                    break;
                case "FoodTruckPreference":
                    queryBuilder.Append("food truck ");
                    break;
                case "CafeteriaPreference":
                    queryBuilder.Append("cafeteria ");
                    break;
                case "DeliveryPreference":
                    queryBuilder.Append("delivery ");
                    break;
                case "AllergiesPreference":
                    queryBuilder.Append("allergy friendly ");
                    break;
                case "DairyFreePreference":
                    queryBuilder.Append("dairy free ");
                    break;
                case "NutFreePreference":
                    queryBuilder.Append("nut free ");
                    break;
                case "SpicyPreference":
                    queryBuilder.Append("spicy ");
                    break;
                case "SweetPreference":
                    queryBuilder.Append("sweet ");
                    break;
                case "SaltyPreference":
                    queryBuilder.Append("salty ");
                    break;
                case "SourPreference":
                    queryBuilder.Append("sour ");
                    break;
                case "BitterPreference":
                    queryBuilder.Append("bitter ");
                    break;
                case "UmamiPreference":
                    queryBuilder.Append("umami ");
                    break;
                case "FusionPreference":
                    queryBuilder.Append("fusion ");
                    break;
            }
            
            return queryBuilder.ToString();
        }

        private async Task<T> FindAndSelectMealWithPreferences<T>(
            string destination, 
            PRICE_LEVEL? priceLevel, 
            string foodQueryPrefix, 
            Random random,
            int dayIndex) where T : class, new()
        {
            string mealQuery = $"{foodQueryPrefix} in {destination}";
            
            var mealList = await _googlePlacesService.SearchPlacesAsync(mealQuery);
            
            if (mealList?.Places == null || mealList.Places.Count == 0)
            {
                MealType mealType = typeof(T).Name switch
                {
                    nameof(Breakfast) => MealType.Breakfast,
                    nameof(Lunch) => MealType.Lunch,
                    nameof(Dinner) => MealType.Dinner,
                    _ => MealType.Breakfast
                };

                string simplifiedQuery = mealType switch
                {
                    MealType.Breakfast => $"{GetPriceQueryPrefix(priceLevel)}{BREAKFAST_QUERY} in {destination}",
                    MealType.Lunch => $"{GetPriceQueryPrefix(priceLevel)}{LUNCH_QUERY} in {destination}",
                    MealType.Dinner => $"{GetPriceQueryPrefix(priceLevel)}{DINNER_QUERY} in {destination}",
                    _ => $"restaurant in {destination}"
                };
                
                mealList = await _googlePlacesService.SearchPlacesAsync(simplifiedQuery);
            }
            
            if (mealList?.Places == null || mealList.Places.Count == 0)
            {
                mealList = await _googlePlacesService.SearchPlacesAsync($"restaurant in {destination}");
            }
            
            if (mealList?.Places != null)
                mealList.Places = FilterUnwantedPlaceTypes(mealList.Places);
            
            if (mealList?.Places == null || mealList.Places.Count == 0)
            {
                throw new InvalidOperationException($"No suitable meal places found in {destination}");
            }
            
            int indexOffset = dayIndex % Math.Max(1, mealList.Places.Count);
            int selectIndex = (random.Next(mealList.Places.Count) + indexOffset) % mealList.Places.Count;
            
            var selectedPlace = mealList.Places[selectIndex];
            
            var placeDetails = await _googlePlacesService.GetPlaceDetailsAsync(selectedPlace.Id);
            T mealPlace = JsonConvert.DeserializeObject<T>(placeDetails);
            
            typeof(T).GetProperty("Id")?.SetValue(mealPlace, Guid.NewGuid());
            typeof(T).GetProperty("_PRICE_LEVEL")?.SetValue(mealPlace, priceLevel);
            typeof(T).GetProperty("GoogleId")?.SetValue(mealPlace, selectedPlace.Id);
            
            typeof(T).GetProperty("UserFoodPreference")?.SetValue(mealPlace, foodQueryPrefix);
            
            return mealPlace;
        }
        
        private (List<PreferenceItem>, List<PreferenceItem>, List<PreferenceItem>) GatherAllPreferences(
            List<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)> allPreferences)
        {
            var foodPrefs = new List<PreferenceItem>();
            var accommodationPrefs = new List<PreferenceItem>();
            var personalizationPrefs = new List<PreferenceItem>();
            
            var hasDominantFoodPreferences = false;
            var dominantFoodPreferences = new List<string>();
            
            foreach (var preferences in allPreferences)
            {
                var accommodationPreference = preferences.Item1;
                var foodPreference = preferences.Item2;
                var personalizationPreference = preferences.Item3;
                
                if (accommodationPreference != null)
                {
                    foreach (var prop in typeof(UserAccommodationPreferences).GetProperties())
                    {
                        if (prop.Name != "UserId" && prop.Name != "Id" && prop.Name != "User" && 
                            prop.PropertyType == typeof(int?))
                        {
                            var value = (int?)prop.GetValue(accommodationPreference);
                            if (value.HasValue && value > 0)
                            {
                                int extraValue = IsDominantAccommodationPreference(prop.Name) ? 50 : 0;
                                
                                accommodationPrefs.Add(new PreferenceItem { 
                                    Name = prop.Name, 
                                    Value = value.Value + extraValue,
                                    IsDominant = IsDominantAccommodationPreference(prop.Name)
                                });
                            }
                        }
                    }
                }
                
                if (foodPreference != null)
                {
                    var userDominantPreferences = CheckForDominantFoodPreferences(foodPreference);
                    
                    if (userDominantPreferences.Count > 0)
                    {
                        hasDominantFoodPreferences = true;
                        dominantFoodPreferences.AddRange(userDominantPreferences);
                    }
                    
                    foreach (var prop in typeof(UserFoodPreferences).GetProperties())
                    {
                        if (prop.Name != "UserId" && prop.Name != "Id" && prop.Name != "User" && 
                            prop.PropertyType == typeof(int?))
                        {
                            var value = (int?)prop.GetValue(foodPreference);
                            if (value.HasValue && value > 0)
                            {
                                bool isDominant = IsDominantFoodPreference(prop.Name);
                                int extraValue = isDominant ? 75 : 0;
                                
                                foodPrefs.Add(new PreferenceItem { 
                                    Name = prop.Name, 
                                    Value = value.Value + extraValue,
                                    IsDominant = isDominant
                                });
                            }
                        }
                    }
                }
                
                if (personalizationPreference != null)
                {
                    foreach (var prop in typeof(UserPersonalization).GetProperties())
                    {
                        if (prop.Name != "UserId" && prop.Name != "Id" && prop.Name != "User" && 
                            prop.PropertyType == typeof(int?))
                        {
                            var value = (int?)prop.GetValue(personalizationPreference);
                            if (value.HasValue && value > 0)
                            {
                                int extraValue = IsDominantPersonalizationPreference(prop.Name) ? 40 : 0;
                                
                                personalizationPrefs.Add(new PreferenceItem { 
                                    Name = prop.Name, 
                                    Value = value.Value + extraValue,
                                    IsDominant = IsDominantPersonalizationPreference(prop.Name)
                                });
                            }
                        }
                    }
                }
            }
            
            if (hasDominantFoodPreferences && dominantFoodPreferences.Count > 0)
            {
                dominantFoodPreferences = dominantFoodPreferences.Distinct().ToList();
                foodPrefs = FilterIncompatibleFoodPreferences(foodPrefs, dominantFoodPreferences);
            }
            
            return (foodPrefs, accommodationPrefs, personalizationPrefs);
        }

        private bool IsDominantFoodPreference(string preferenceName)
        {
            var dominantPreferences = new HashSet<string>
            {
                "VeganPreference",
                "VegetarianPreference",
                "HalalPreference",
                "KosherPreference",
                "GlutenFreePreference",
                "DairyFreePreference",
                "NutFreePreference",
                "AllergiesPreference"
            };
            
            return dominantPreferences.Contains(preferenceName);
        }
        
        private bool IsDominantAccommodationPreference(string preferenceName)
        {
            var dominantPreferences = new HashSet<string>
            {
                "PetFriendlyPreference",
                "FamilyFriendlyPreference",
                "AdultsOnlyPreference",
                "EcoFriendlyPreference"
            };
            
            return dominantPreferences.Contains(preferenceName);
        }
        
        private bool IsDominantPersonalizationPreference(string preferenceName)
        {
            var dominantPreferences = new HashSet<string>
            {
                "AdventurePreference",
                "FamilyTravelPreference",
                "LuxuryPreference",
                "BudgetPreference"
            };
            
            return dominantPreferences.Contains(preferenceName);
        }
        
        private List<string> CheckForDominantFoodPreferences(UserFoodPreferences? preferences)
        {
            var dominantPreferences = new List<string>();
            
            if (preferences == null)
                return dominantPreferences;
                
            if (preferences.VeganPreference.HasValue && preferences.VeganPreference.Value > 0)
                dominantPreferences.Add("VeganPreference");
                
            if (preferences.VegetarianPreference.HasValue && preferences.VegetarianPreference.Value > 75)
                dominantPreferences.Add("VegetarianPreference");
                
            if (preferences.HalalPreference.HasValue && preferences.HalalPreference.Value > 0)
                dominantPreferences.Add("HalalPreference");
                
            if (preferences.KosherPreference.HasValue && preferences.KosherPreference.Value > 0)
                dominantPreferences.Add("KosherPreference");
                
            if (preferences.GlutenFreePreference.HasValue && preferences.GlutenFreePreference.Value > 50)
                dominantPreferences.Add("GlutenFreePreference");
                
            if (preferences.DairyFreePreference.HasValue && preferences.DairyFreePreference.Value > 50)
                dominantPreferences.Add("DairyFreePreference");
                
            if (preferences.NutFreePreference.HasValue && preferences.NutFreePreference.Value > 50)
                dominantPreferences.Add("NutFreePreference");
                
            if (preferences.AllergiesPreference.HasValue && preferences.AllergiesPreference.Value > 0)
                dominantPreferences.Add("AllergiesPreference");
                
            return dominantPreferences;
        }
        
        private List<PreferenceItem> FilterIncompatibleFoodPreferences(List<PreferenceItem> allPreferences, List<string> dominantPreferences)
        {
            var result = new List<PreferenceItem>(allPreferences);
            
            if (dominantPreferences.Contains("VeganPreference"))
            {
                result.RemoveAll(p => p.Name == "SeafoodPreference");
                
                var veganPref = result.FirstOrDefault(p => p.Name == "VeganPreference");
                if (veganPref != null)
                {
                    veganPref.Value = 1000; 
                    veganPref.IsMandatory = true; 
                }
                
                foreach (var pref in result.Where(p => p.Name == "VegetarianPreference" || 
                                                    p.Name == "OrganicPreference" || 
                                                    p.Name == "GlutenFreePreference"))
                {
                    pref.Value = Math.Min(pref.Value * 2, 200);
                }
            }
            
            if (dominantPreferences.Contains("VegetarianPreference"))
            {
                result.RemoveAll(p => p.Name == "SeafoodPreference");
                
                var vegPref = result.FirstOrDefault(p => p.Name == "VegetarianPreference");
                if (vegPref != null)
                {
                    vegPref.Value = 500; 
                    vegPref.IsMandatory = true; 
                }
            }
            
            if (dominantPreferences.Contains("HalalPreference"))
            {
                var halalPref = result.FirstOrDefault(p => p.Name == "HalalPreference");
                if (halalPref != null)
                {
                    halalPref.Value = 1000; 
                    halalPref.IsMandatory = true; 
                }
            }
            
            if (dominantPreferences.Contains("KosherPreference"))
            {
                var kosherPref = result.FirstOrDefault(p => p.Name == "KosherPreference");
                if (kosherPref != null)
                {
                    kosherPref.Value = 1000; 
                    kosherPref.IsMandatory = true; 
                }
            }
            
            if (dominantPreferences.Contains("GlutenFreePreference"))
            {
                var glutenFreePref = result.FirstOrDefault(p => p.Name == "GlutenFreePreference");
                if (glutenFreePref != null)
                {
                    glutenFreePref.Value = 800;
                    glutenFreePref.IsMandatory = true; 
                }
            }
            
            if (dominantPreferences.Contains("DairyFreePreference"))
            {
                var dairyFreePref = result.FirstOrDefault(p => p.Name == "DairyFreePreference");
                if (dairyFreePref != null)
                {
                    dairyFreePref.Value = 800;
                    dairyFreePref.IsMandatory = true; 
                }
            }
            
            if (dominantPreferences.Contains("NutFreePreference"))
            {
                var nutFreePref = result.FirstOrDefault(p => p.Name == "NutFreePreference");
                if (nutFreePref != null)
                {
                    nutFreePref.Value = 800;
                    nutFreePref.IsMandatory = true; 
                }
            }
            
            if (dominantPreferences.Contains("AllergiesPreference"))
            {
                var allergyPref = result.FirstOrDefault(p => p.Name == "AllergiesPreference");
                if (allergyPref != null)
                {
                    allergyPref.Value = 1000;
                    allergyPref.IsMandatory = true;
                }
            }
            
            foreach (var preference in result.Where(p => p.IsDominant && !p.IsMandatory))
            {
                preference.Value = Math.Max(preference.Value, 100); 
            }
            
            return result;
        }

        public async Task<GeminiResponse> GetGeminiDataForRouteAsync(StandardRoute? route,DateOnly? startDate,DateOnly? endDate) 
        {
            if (route == null)
                return null;
                
            var currency = route.Currency; 
            var hotelname = route.TravelDays?[route.TravelDays.Count - 1]?.Accomodation?.DisplayName?.Text;
            var destination = route.City;
            var dayCount = route.TravelDays?.Count ?? 0;
            
            var placeNames = new List<List<string>>();
            
            for (int i = 0; i < dayCount; i++)
            {
                var day = route.TravelDays[i];
                var dayPlaces = new List<string>
                {
                    day?.Accomodation?.DisplayName?.Text ?? "Hotel",
                    day?.Breakfast?.DisplayName?.Text ?? "Breakfast Place",
                    day?.Lunch?.DisplayName?.Text ?? "Lunch Place",
                    day?.Dinner?.DisplayName?.Text ?? "Dinner Place",
                    day?.FirstPlace?.DisplayName?.Text ?? "First Attraction",
                    day?.SecondPlace?.DisplayName?.Text ?? "Second Attraction",
                    day?.ThirdPlace?.DisplayName?.Text ?? "Third Attraction",
                    day?.PlaceAfterDinner?.DisplayName?.Text ?? "Evening Venue"
                };
                
                placeNames.Add(dayPlaces);
            }
            
            var placeNamesJson = System.Text.Json.JsonSerializer.Serialize(placeNames);
            
            var prompt = $@"Please provide the following information for this route in JSON format:
            
            1. Approximate spending amount for each day in {currency} ('approxPrices' array) - include {dayCount} days worth of prices. Format should be ""{currency} 100"" (with a space between currency and amount)
               IMPORTANT FOR PRICING: You must calculate a realistic total daily cost by adding up expenses for:
               - Accommodation at the specific hotel listed for each day
               - All meals (breakfast, lunch, dinner) at the specific restaurants listed
               - Entrance fees for all three attractions/places visited that day
               - Transportation between all locations throughout the day
               - Evening entertainment/activities at the after-dinner venue
               - Incidental expenses
               Consider the quality and location of each specific place when estimating its cost. Be realistic and thorough in your calculations, accounting for all expenses a traveler would actually encounter.
            
            2. Star rating for accommodation '{hotelname}' ('star' string)
            
            3. Daily descriptions ('dayDescriptions' array) - Write a 3-4 sentence description for each day's itinerary based on the place names. Be engaging and descriptive. Include {dayCount} descriptions.
            
            4. Weather forecast for EACH DAY of the route. For each day, provide 7 weather entries at different times of the day ('weathers' array of arrays).
            
            The route is in {destination} for {dayCount} days, from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}.
            
            Here are the place names for each day of the route:
            {placeNamesJson}
            
            IMPORTANT WEATHER INSTRUCTIONS:
            - Each day should have 7 weather entries corresponding to different times
            - Weather entries for each day should follow this time sequence:
              1. 09:00 AM (morning, breakfast time)
              2. 11:00 AM (late morning, first place visit)
              3. 13:00 PM (early afternoon, lunch time)
              4. 15:00 PM (afternoon, second place visit)
              5. 17:00 PM (late afternoon, third place visit)
              6. 19:00 PM (evening, dinner time)
              7. 21:00 PM (night, after dinner activities)
            - For a {dayCount}-day route, your 'weathers' array should contain {dayCount} subarrays, each with 7 weather entries
            - IMPORTANT: In the 'Date' field of weather entries, include ONLY the date in yyyy-MM-dd format. DO NOT include the time.
            
            Return the answer in this format:
            
            {{
              ""approxPrices"": [""{currency} 100"", ""{currency} 120"", ...], // {dayCount} prices, one for each day, WITH SPACE between currency and amount
              ""star"": ""4"",
              ""dayDescriptions"": [
                ""Day 1: Begin your day with a delightful breakfast at [Breakfast Place]. Visit the magnificent [First Attraction] and explore the historic [Second Attraction]. Enjoy lunch at [Lunch Place] before discovering [Third Attraction]. End your day with dinner at [Dinner Place] followed by entertainment at [Evening Venue]."",
                ""Day 2: ..."",
                // Continue for all {dayCount} days
              ],
              ""weathers"": [
                [ // Day 1 weathers (7 entries)
                  {{
                    ""Degree"": 25,
                    ""Description"": ""Sunny, light breeze"",
                    ""Warning"": ""Don't forget sunscreen"",
                    ""Date"": ""2023-07-01"" // Date only, no time
                  }},
                  // 6 more weather entries for day 1
                ],
                // Continue for all {dayCount} days
              ]
            }}";
            
            try 
            {
                var response = await _geminiAIService.GenerateContentAsync(prompt);
                
                response = response.Replace("```json", "").Replace("```", "").Trim();
                
                var geminiResponse = System.Text.Json.JsonSerializer.Deserialize<GeminiResponse>(response, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
                return geminiResponse;
            }
            catch(Exception ex)
            {
                return null;
            }
        }

        public class GeminiResponse 
        {
            public List<string>? approxPrices { get; set; } // per travel day
            public string? star { get; set; } //one accomodation hotel star
            public List<List<Weather>>? weathers { get; set; } //per place ,7 per day
            public List<string>? dayDescriptions { get; set; } //per travel day
 
        }

        private string ConvertPreferenceNameToKeyword(string preferenceName)
        {
            if (preferenceName.EndsWith("Preference"))
            {
                return preferenceName.Substring(0, preferenceName.Length - 10);
            }
            return preferenceName;
        }

        /*
* {
"StatusCode": 500,
"Message": "Response status code does not indicate success: 502 (Bad Gateway).",
"Title": "Error",
"EnumStatusCode": 999
   }
*/
    }
}
