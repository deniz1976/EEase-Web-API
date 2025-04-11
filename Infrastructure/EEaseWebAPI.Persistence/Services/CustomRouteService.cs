using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Services
{
    public class CustomRouteService : ICustomRouteService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IGeminiAIService _geminiAIService;
        private readonly IGooglePlacesService _googlePlacesService;
        private readonly EEaseAPIDbContext _context;

        private readonly static string EXPENSIVE_HOTEL_QUERY = "4 or 5";
        private readonly static string MODERATE_HOTEL_QUERY = "4";
        private readonly static string INEXPENSIVE_HOTEL_QUERY = "2 or 3";
        private readonly static string VERY_EXPENSIVE_HOTEL_QUERY = "Luxury 5";

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



        public CustomRouteService(UserManager<AppUser> userManager, IUserService userService, IGeminiAIService geminiAIService, IGooglePlacesService googlePlacesService, EEaseAPIDbContext context)
        {
            _userManager = userManager;
            _userService = userService;
            _geminiAIService = geminiAIService;
            _googlePlacesService = googlePlacesService;
            _context = context;
        }

        /// <summary>
        /// Creates a random route based on the specified destination, date range, and price level.
        /// </summary>
        /// <param name="destination">Destination city</param>
        /// <param name="startDate">Start date</param>
        /// <param name="endDate">End date</param>
        /// <param name="_PRICE_LEVEL">Price level (Inexpensive, Moderate, Expensive, Very Expensive)</param>
        /// <returns>Generated standard route</returns>
        /// <exception cref="ArgumentNullException">Thrown when required parameters are null</exception>
        /// <exception cref="InvalidOperationException">Thrown when not enough unique places are found</exception>
        public async Task<StandardRoute> CreateRandomRoute(string destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL)
        {
            Random random = new Random();
            destination = char.ToUpper(destination[0]) + destination[1..].ToLowerInvariant();

            if (destination == null || startDate == null || endDate == null)
                throw new ArgumentNullException();

            var dayCount = (endDate.Value.DayNumber - startDate.Value.DayNumber);
            if (dayCount == 0) dayCount = 1;

            var admin = await _userManager.FindByNameAsync("admin");

            StandardRoute standardRoute = new StandardRoute() 
            {
            City = destination,
            User = null,
            LikedUsers = new List<AppUser>(),
            name = destination,
            UserId = admin.Id,
            Days = dayCount,
            Id = Guid.NewGuid(),
            LikeCount = 0,
                TravelDays = new List<TravelDay>(),
                status = 2,
                
            };

            for(int i = 0; i < dayCount; i++)
            {
                standardRoute.TravelDays.Add(new TravelDay());
            }

            string queryPrefix = _PRICE_LEVEL switch
            {
                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => EXPENSIVE_HOTEL_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_MODERATE => MODERATE_HOTEL_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => INEXPENSIVE_HOTEL_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => VERY_EXPENSIVE_HOTEL_QUERY,
                _ => EXPENSIVE_HOTEL_QUERY
            };

            string hotelQuery = $"{queryPrefix} star hotel in {destination}";
            var hotelList = await _googlePlacesService.SearchPlacesAsync(hotelQuery);
            int select = random.Next(hotelList.Places.Count);
            var hotel = hotelList?.Places?[select];

            var hotelDetails = await _googlePlacesService.GetPlaceDetailsAsync(hotel.Id);
            TravelAccomodation? accomodation = JsonConvert.DeserializeObject<TravelAccomodation>(hotelDetails);
            accomodation._PRICE_LEVEL = _PRICE_LEVEL;
            accomodation.Star = queryPrefix;
            accomodation.UserAccomodationPreference = "random";

            string breakfastQueryPrefix = _PRICE_LEVEL switch
            {
                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => EXPENSIVE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_MODERATE => MODERATE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => INEXPENSIVE_QUERY,
                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => VERY_EXPENSIVE_QUERY,
                _ => MODERATE_QUERY
            };

            var breakfastPlaces = await _googlePlacesService.SearchPlacesAsync($"{breakfastQueryPrefix}{BREAKFAST_QUERY}in {destination}");
            var lunchPlaces = await _googlePlacesService.SearchPlacesAsync($"{breakfastQueryPrefix}{LUNCH_QUERY}in {destination}");
            var dinnerPlaces = await _googlePlacesService.SearchPlacesAsync($"{breakfastQueryPrefix}{DINNER_QUERY}in {destination}");

            if (breakfastPlaces.Places.Count < dayCount)
            {
                var additionalPrefix = GetNextPriceLevel(breakfastQueryPrefix, _PRICE_LEVEL);
                var additionalBreakfastPlaces = await _googlePlacesService.SearchPlacesAsync($"{additionalPrefix}{BREAKFAST_QUERY}in {destination}");
                breakfastPlaces.Places.AddRange(additionalBreakfastPlaces.Places);
            }

            if (lunchPlaces.Places.Count < dayCount)
            {
                var additionalPrefix = GetNextPriceLevel(breakfastQueryPrefix, _PRICE_LEVEL);
                var additionalLunchPlaces = await _googlePlacesService.SearchPlacesAsync($"{additionalPrefix}{LUNCH_QUERY}in {destination}");
                lunchPlaces.Places.AddRange(additionalLunchPlaces.Places);
            }

            if (dinnerPlaces.Places.Count < dayCount)
            {
                var additionalPrefix = GetNextPriceLevel(breakfastQueryPrefix, _PRICE_LEVEL);
                var additionalDinnerPlaces = await _googlePlacesService.SearchPlacesAsync($"{additionalPrefix}{DINNER_QUERY}in {destination}");
                dinnerPlaces.Places.AddRange(additionalDinnerPlaces.Places);
            }

            var touristicQueries = new[] { TOURISTIC_QUERY, TOURISTIC_QUERY1, TOURISTIC_QUERY2, TOURISTIC_QUERY3, TOURISTIC_QUERY4 };
            var allTouristicPlaces = new List<PlaceSearchResponse>();
            
            foreach (var query in touristicQueries)
            {
                var places = await _googlePlacesService.SearchPlacesAsync($"{query}in {destination}");
                if (places?.Places != null)
                {
                    allTouristicPlaces.Add(places);
                }
            }

            var breakfastGoogleIds = breakfastPlaces.Places.Select(p => p.Id).ToList();
            var lunchGoogleIds = lunchPlaces.Places.Select(p => p.Id).ToList();
            var dinnerGoogleIds = dinnerPlaces.Places.Select(p => p.Id).ToList();
            var touristicGoogleIds = allTouristicPlaces.SelectMany(p => p.Places).Select(p => p.Id).ToList();

            var afterDinnerQueries = new[] { AFTER_DINNER_QUERY1, AFTER_DINNER_QUERY2, AFTER_DINNER_QUERY3, AFTER_DINNER_QUERY4, AFTER_DINNER_QUERY5 };
            var afterDinnerPlaces = new List<PlaceSearchResponse>();
            var afterDinnerGoogleIds = new List<string>();

            foreach (var query in afterDinnerQueries)
            {
                var places = await _googlePlacesService.SearchPlacesAsync($"{breakfastQueryPrefix}{query}in {destination}");
                if (places?.Places != null)
                {
                    afterDinnerPlaces.Add(places);
                    afterDinnerGoogleIds.AddRange(places.Places.Select(p => p.Id));
                }
            }

            if (afterDinnerGoogleIds.Count < dayCount)
            {
                var additionalPrefix = GetNextPriceLevel(breakfastQueryPrefix, _PRICE_LEVEL);
                foreach (var query in afterDinnerQueries)
                {
                    var places = await _googlePlacesService.SearchPlacesAsync($"{additionalPrefix}{query}in {destination}");
                    if (places?.Places != null)
                    {
                        afterDinnerPlaces.Add(places);
                        afterDinnerGoogleIds.AddRange(places.Places.Select(p => p.Id));
                    }
                }
            }

            var usedBreakfastIndices = new HashSet<int>();
            var usedLunchIndices = new HashSet<int>();
            var usedDinnerIndices = new HashSet<int>();
            var usedAfterDinnerIndices = new HashSet<int>();
            var usedTouristicIndices = new HashSet<int>();
            var usedGoogleIds = new HashSet<string>();

            if (breakfastPlaces.Places.Count < dayCount || 
                lunchPlaces.Places.Count < dayCount || 
                dinnerPlaces.Places.Count < dayCount || 
                touristicGoogleIds.Count < (dayCount * 3))
            {
                throw new InvalidOperationException("Not enough unique places found even after trying different price levels.");
            }

            for (int i = 0; i < dayCount; i++)
            {
                var day = new TravelDay
                {
                    Accomodation = new TravelAccomodation
                    {
                        Id = Guid.NewGuid(),
                        InternationalPhoneNumber = accomodation.InternationalPhoneNumber,
                        NationalPhoneNumber = accomodation.NationalPhoneNumber,
                        FormattedAddress = accomodation.FormattedAddress,
                        Rating = accomodation.Rating,
                        GoogleMapsUri = accomodation.GoogleMapsUri,
                        WebsiteUri = accomodation.WebsiteUri,
                        GoodForChildren = accomodation.GoodForChildren,
                        Restroom = accomodation.Restroom,
                        PrimaryType = accomodation.PrimaryType,
                        GoogleId = accomodation.GoogleId,
                        Location = accomodation.Location,
                        RegularOpeningHours = accomodation.RegularOpeningHours,
                        DisplayName = accomodation.DisplayName,
                        Photos = accomodation.Photos,
                        PaymentOptions = accomodation.PaymentOptions,
                        _PRICE_LEVEL = accomodation._PRICE_LEVEL,
                        UserAccomodationPreference = "random + " + i,
                        UserPersonalizationPref = null
                    }
                };

                int breakfastIndex;
                do
                {
                    breakfastIndex = random.Next(breakfastPlaces.Places.Count); // buralarda bazen sonsuz döngü todo
                } while (usedBreakfastIndices.Contains(breakfastIndex) || 
                        usedGoogleIds.Contains(breakfastGoogleIds[breakfastIndex]));
                usedBreakfastIndices.Add(breakfastIndex);
                usedGoogleIds.Add(breakfastGoogleIds[breakfastIndex]);
                
                var breakfastDetails = await _googlePlacesService.GetPlaceDetailsAsync(breakfastGoogleIds[breakfastIndex]);
                day.Breakfast = JsonConvert.DeserializeObject<Breakfast>(breakfastDetails);
                day.Breakfast.Id = Guid.NewGuid();
                day.Breakfast._PRICE_LEVEL = _PRICE_LEVEL;
                day.Breakfast.GoogleId = breakfastGoogleIds[breakfastIndex];

                int lunchIndex;
                do
                {
                    lunchIndex = random.Next(lunchPlaces.Places.Count);
                } while (usedLunchIndices.Contains(lunchIndex) || 
                        usedGoogleIds.Contains(lunchGoogleIds[lunchIndex]));
                usedLunchIndices.Add(lunchIndex);
                usedGoogleIds.Add(lunchGoogleIds[lunchIndex]);

                var lunchDetails = await _googlePlacesService.GetPlaceDetailsAsync(lunchGoogleIds[lunchIndex]);
                day.Lunch = JsonConvert.DeserializeObject<Lunch>(lunchDetails);
                day.Lunch.Id = Guid.NewGuid();
                day.Lunch._PRICE_LEVEL = _PRICE_LEVEL;
                day.Lunch.GoogleId = lunchGoogleIds[lunchIndex];

                int dinnerIndex;
                do
                {
                    dinnerIndex = random.Next(dinnerPlaces.Places.Count);
                } while (usedDinnerIndices.Contains(dinnerIndex) || 
                        usedGoogleIds.Contains(dinnerGoogleIds[dinnerIndex]));
                usedDinnerIndices.Add(dinnerIndex);
                usedGoogleIds.Add(dinnerGoogleIds[dinnerIndex]);

                var dinnerDetails = await _googlePlacesService.GetPlaceDetailsAsync(dinnerGoogleIds[dinnerIndex]);
                day.Dinner = JsonConvert.DeserializeObject<Dinner>(dinnerDetails);
                day.Dinner.Id = Guid.NewGuid();
                day.Dinner._PRICE_LEVEL = _PRICE_LEVEL;
                day.Dinner.GoogleId = dinnerGoogleIds[dinnerIndex];

                var selectedAfterDinnerList = afterDinnerPlaces[random.Next(afterDinnerPlaces.Count)];
                int afterDinnerIndex;
                do
                {
                    afterDinnerIndex = random.Next(selectedAfterDinnerList.Places.Count);
                } while (usedAfterDinnerIndices.Contains(afterDinnerIndex) || 
                        usedGoogleIds.Contains(afterDinnerGoogleIds[afterDinnerIndex]));
                usedAfterDinnerIndices.Add(afterDinnerIndex);
                usedGoogleIds.Add(afterDinnerGoogleIds[afterDinnerIndex]);

                var afterDinnerDetails = await _googlePlacesService.GetPlaceDetailsAsync(afterDinnerGoogleIds[afterDinnerIndex]);
                day.PlaceAfterDinner = JsonConvert.DeserializeObject<PlaceAfterDinner>(afterDinnerDetails);
                day.PlaceAfterDinner.Id = Guid.NewGuid();
                day.PlaceAfterDinner._PRICE_LEVEL = _PRICE_LEVEL;
                day.PlaceAfterDinner.GoogleId = afterDinnerGoogleIds[afterDinnerIndex];

                int firstPlaceIndex;
                do
                {
                    firstPlaceIndex = random.Next(touristicGoogleIds.Count);
                } while (usedTouristicIndices.Contains(firstPlaceIndex) || 
                        usedGoogleIds.Contains(touristicGoogleIds[firstPlaceIndex]));
                usedTouristicIndices.Add(firstPlaceIndex);
                usedGoogleIds.Add(touristicGoogleIds[firstPlaceIndex]);

                var firstPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[firstPlaceIndex]);
                day.FirstPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(firstPlaceDetails);
                day.FirstPlace.Id = Guid.NewGuid();
                day.FirstPlace.GoogleId = touristicGoogleIds[firstPlaceIndex];

                int secondPlaceIndex;
                do
                {
                    secondPlaceIndex = random.Next(touristicGoogleIds.Count);
                } while (usedTouristicIndices.Contains(secondPlaceIndex) || 
                        usedGoogleIds.Contains(touristicGoogleIds[secondPlaceIndex]));
                usedTouristicIndices.Add(secondPlaceIndex);
                usedGoogleIds.Add(touristicGoogleIds[secondPlaceIndex]);

                var secondPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[secondPlaceIndex]);
                day.SecondPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(secondPlaceDetails);
                day.SecondPlace.Id = Guid.NewGuid();
                day.SecondPlace.GoogleId = touristicGoogleIds[secondPlaceIndex];

                int thirdPlaceIndex;
                do
                {
                    thirdPlaceIndex = random.Next(touristicGoogleIds.Count);
                } while (usedTouristicIndices.Contains(thirdPlaceIndex) || 
                        usedGoogleIds.Contains(touristicGoogleIds[thirdPlaceIndex]));
                usedTouristicIndices.Add(thirdPlaceIndex);
                usedGoogleIds.Add(touristicGoogleIds[thirdPlaceIndex]);

                var thirdPlaceDetails = await _googlePlacesService.GetPlaceDetailsAsync(touristicGoogleIds[thirdPlaceIndex]);
                day.ThirdPlace = JsonConvert.DeserializeObject<Domain.Entities.Route.Place>(thirdPlaceDetails);
                day.ThirdPlace.Id = Guid.NewGuid();
                day.ThirdPlace.GoogleId = touristicGoogleIds[thirdPlaceIndex];

                standardRoute.TravelDays[i] = day;
            }

            

            await _context.StandardRoutes.AddAsync(standardRoute);
            await _context.SaveChangesAsync();
            standardRoute.User = null;


            Console.WriteLine(RouteTest(standardRoute));
            PrintRoute(standardRoute);
            return standardRoute;
        }

        /// <summary>
        /// Returns the next price level based on the current price level.
        /// </summary>
        /// <param name="currentPrefix">Current price level prefix</param>
        /// <param name="currentLevel">Current price level</param>
        /// <returns>Next price level query prefix</returns>
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

        private void PrintRoute(StandardRoute r) 
        {
            Console.WriteLine(r.TravelDays?[0]?.Accomodation?.DisplayName?.Text);
            for (int i = 0; i < r.TravelDays?.Count; i++)
            {
                Console.WriteLine(r.TravelDays?[i]?.Breakfast?.DisplayName?.Text);
                Console.WriteLine(r.TravelDays?[i]?.Lunch?.DisplayName?.Text);
                Console.WriteLine(r.TravelDays?[i]?.Dinner?.DisplayName?.Text);
                Console.WriteLine(r.TravelDays?[i]?.FirstPlace?.DisplayName?.Text);
                Console.WriteLine(r.TravelDays?[i]?.SecondPlace?.DisplayName?.Text);
                Console.WriteLine(r.TravelDays?[i]?.ThirdPlace?.DisplayName?.Text);
                Console.WriteLine(r.TravelDays?[i]?.PlaceAfterDinner?.DisplayName?.Text);

            }
        }

        private bool RouteTest(StandardRoute route)
        {
            int lengthShouldBe = route.TravelDays.Count * 7;
            var hashmap = new Dictionary<string, int>();

            try
            {
                for (int i = 0; i < route.TravelDays.Count; i++)
                {
                    if (!string.IsNullOrEmpty(route.TravelDays[i].Breakfast.GoogleId))
                        hashmap[route.TravelDays[i].Breakfast.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(route.TravelDays[i].Lunch.GoogleId))
                        hashmap[route.TravelDays[i].Lunch.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(route.TravelDays[i].Dinner.GoogleId))
                        hashmap[route.TravelDays[i].Dinner.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(route.TravelDays[i].FirstPlace.GoogleId))
                        hashmap[route.TravelDays[i].FirstPlace.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(route.TravelDays[i].SecondPlace.GoogleId))
                        hashmap[route.TravelDays[i].SecondPlace.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(route.TravelDays[i].ThirdPlace.GoogleId))
                        hashmap[route.TravelDays[i].ThirdPlace.GoogleId] = 1;

                    if (!string.IsNullOrEmpty(route.TravelDays[i].PlaceAfterDinner.GoogleId))
                        hashmap[route.TravelDays[i].PlaceAfterDinner.GoogleId] = 1;
                }

                if (hashmap.Count < lengthShouldBe)
                {
                    return false;
                }
                return true;
            }
            catch
            {
                return false;
            }
        }



    }   
}

