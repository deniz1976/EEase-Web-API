using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Microsoft.EntityFrameworkCore;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using System;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Application.MapEntities;

/// https://www.instagram.com/p/C-jNm9OKX9j/
namespace EEaseWebAPI.Persistence.Services
{
    /// <summary>
    /// Provides functionality for managing travel routes and itineraries.
    /// Handles route creation, modification, and retrieval operations.
    /// </summary>
    public class RouteService : IRouteService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserService _userService;
        private readonly IGeminiAIService _geminiAIService;
        private readonly IGooglePlacesService _googlePlacesService;
        private readonly EEaseAPIDbContext _context;

        /// <summary>
        /// Initializes a new instance of the RouteService class.
        /// </summary>
        /// <param name="userManager">User management service for handling user-related operations</param>
        /// <param name="userService">Service for user-specific functionality</param>
        /// <param name="geminiAIService">AI service for generating travel recommendations</param>
        /// <param name="googlePlacesService">Service for accessing Google Places data</param>
        /// <param name="context">Database context for saving route data</param>
        public RouteService(
            UserManager<AppUser> userManager, 
            IUserService userService, 
            IGeminiAIService geminiAIService, 
            IGooglePlacesService googlePlacesService,
            EEaseAPIDbContext context)
        {
            _userManager = userManager;
            _userService = userService;
            _geminiAIService = geminiAIService;
            _googlePlacesService = googlePlacesService;
            _context = context;
        }

        /// <summary>
        /// Creates a custom travel route based on user preferences and AI recommendations.
        /// This method:
        /// 1. Retrieves user preferences for accommodation, food, and personalization.
        /// 2. Combines preferences from multiple users.
        /// 3. Generates a travel itinerary using AI services.
        /// 4. Retrieves detailed place information from Google Places API.
        /// 5. Constructs a structured route with accommodations, meals, and activities.
        /// 6. Saves the route to the database.
        /// </summary>
        /// <param name="destination">The destination city or location for the route.</param>
        /// <param name="startDate">The start date of the travel period.</param>
        /// <param name="endDate">The end date of the travel period.</param>
        /// <param name="_PRICE_LEVEL">The preferred price level for accommodations and activities.</param>
        /// <param name="username">The username of the head user creating the route.</param>
        /// <param name="usernames">A list of usernames for additional users included in the route.</param>
        /// <returns>A response containing the complete route details including places, descriptions, and metadata.</returns>
        /// <exception cref="UserNotFoundException">Thrown when a specified user is not found.</exception>
        public async Task<CreateCustomRouteCommandResponseBody> CreateCustomRoute(
            string? destination, 
            DateOnly? startDate, 
            DateOnly? endDate, 
            PRICE_LEVEL? _PRICE_LEVEL, 
            string? username, 
            List<string>? usernames)
        {
            destination = char.ToUpper(destination[0]) + destination[1..].ToLowerInvariant();
            var dayCount = (endDate.Value.DayNumber - startDate.Value.DayNumber);
            if (dayCount == 0) dayCount = 1;

            var headuser = await _userManager.FindByNameAsync(username);
            if (headuser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("Head user not found", 7);

            var allUserPreferences = new List<(UserAccommodationPreferences Accommodation, 
                                     UserFoodPreferences Food, 
                                     UserPersonalization Personal)>();

            var headUserAccommodationPrefs = await _context.UserAccommodationPreferences
                .FirstOrDefaultAsync(p => p.UserId == headuser.Id);
            var headUserFoodPrefs = await _context.UserFoodPreferences
                .FirstOrDefaultAsync(p => p.UserId == headuser.Id);
            var headUserPersonalization = await _context.UserPersonalizations
                .FirstOrDefaultAsync(p => p.UserId == headuser.Id);

            if (headUserAccommodationPrefs != null && headUserFoodPrefs != null && headUserPersonalization != null)
            {
                allUserPreferences.Add((
                    headUserAccommodationPrefs,
                    headUserFoodPrefs,
                    headUserPersonalization
                ));
            }

            if (usernames != null)
            {
                foreach (var otherUsername in usernames)
                {
                    var otherUser = await _userManager.FindByNameAsync(otherUsername);
                    if (otherUser == null)
                        throw new Application.Exceptions.Login.UserNotFoundException($"User {otherUsername} not found", 7);

                    var userAccommodationPrefs = await _context.UserAccommodationPreferences
                        .FirstOrDefaultAsync(p => p.UserId == otherUser.Id);
                    var userFoodPrefs = await _context.UserFoodPreferences
                        .FirstOrDefaultAsync(p => p.UserId == otherUser.Id);
                    var userPersonalization = await _context.UserPersonalizations
                        .FirstOrDefaultAsync(p => p.UserId == otherUser.Id);

                    if (userAccommodationPrefs != null && userFoodPrefs != null && userPersonalization != null)
                    {
                        allUserPreferences.Add((
                            userAccommodationPrefs,
                            userFoodPrefs,
                            userPersonalization
                        ));
                    }
                }
            }

            var combinedPreferences = CombineUserPreferences(allUserPreferences);
            var combinedPref1 = CombineUserPreferencesForCustoms(allUserPreferences, dayCount);

            var temp = CreateCustomRouteWithPreferences(destination, startDate, endDate, _PRICE_LEVEL, username, usernames);

            var days = await _geminiAIService.CreateCustomRouteWithPreferences(
                destination, 
                dayCount, 
                startDate, 
                endDate, 
                _PRICE_LEVEL,
                combinedPreferences.Item1,
                combinedPreferences.Item2,
                combinedPreferences.Item3);

            var standardRoute = new StandardRoute
            {
                City = destination,
                LikedUsers = new List<AppUser>(),
                name = destination,
                UserId = headuser.Id,
                Days = dayCount,
                LikeCount = 0,
                TravelDays = new List<TravelDay>(dayCount),
                UserAccommodationPreferences = null,
                UserFoodPreferences = null,
                UserPersonalization = null,
                status = 0
            };

            for (int i = 0; i < days.Count; i++)
            {
                var currentDate = startDate.Value.AddDays(i);
                
                TravelDay travelDay = new TravelDay
                {
                    DayDescription = days[i].DayDescription,
                    User = headuser,
                    approxPrice = days[i].ApproxPrice != null ? double.Parse(days[i].ApproxPrice) : 1000
                };


                var morningWeather = await _geminiAIService.GetWeatherForDateAsync(destination, currentDate, new TimeOnly(8, 0));
                morningWeather.Id = new Guid();

                var noonWeather = await _geminiAIService.GetWeatherForDateAsync(destination, currentDate, new TimeOnly(12, 0));
                noonWeather.Id = new Guid();
                var eveningWeather = await _geminiAIService.GetWeatherForDateAsync(destination, currentDate, new TimeOnly(19, 0));
                eveningWeather.Id = new Guid();

                //var morningWeatherTask = _geminiAIService.GetWeatherForDateAsync(destination, currentDate, new TimeOnly(8, 0));
                //var noonWeatherTask = _geminiAIService.GetWeatherForDateAsync(destination, currentDate, new TimeOnly(12, 0));
                //var eveningWeatherTask = _geminiAIService.GetWeatherForDateAsync(destination, currentDate, new TimeOnly(19, 0));

                //await Task.WhenAll(morningWeatherTask, noonWeatherTask, eveningWeatherTask);

                //var morningWeather = morningWeatherTask.Result;
                //var noonWeather = noonWeatherTask.Result;
                //var eveningWeather = eveningWeatherTask.Result;

                // ID'leri atama
                //morningWeather.Id = Guid.NewGuid();
                //noonWeather.Id = Guid.NewGuid();
                //eveningWeather.Id = Guid.NewGuid();


                var accoDetails = await GetPlaceDetails(days[i].AccomodationPlaceName, destination);
                var accoDetails1 = await GetAccomodationDetails(days[i].AccomodationPlaceName, destination);
                //accoDetails1.UserAccommodationPreferences = combinedPreferences.Item1;

                if (accoDetails != null)
                {
                    var acc1Guid = Guid.NewGuid();
                    var accommodation = new TravelAccomodation
                    {
                        Id = acc1Guid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        Star = accoDetails["star"]?.Value<string>(),
                        GoogleId = accoDetails["id"]?.ToString(),
                        FormattedAddress = accoDetails["formattedAddress"]?.ToString(),
                        Rating = accoDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = accoDetails["googleMapsUri"]?.ToString(),
                        GoodForChildren = accoDetails["goodForChildren"]?.Value<bool>(),
                        Restroom = accoDetails["restroom"]?.Value<bool>(),
                        WebsiteUri = accoDetails["websiteUri"]?.ToString(),
                        InternationalPhoneNumber = accoDetails["internationalPhoneNumber"]?.ToString(),
                        NationalPhoneNumber = accoDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = accoDetails["primaryType"]?.ToString(),
                        DisplayName = accoDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = accoDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = accoDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = accoDetails["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = accoDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = accoDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        Photos = accoDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        RegularOpeningHours = accoDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = accoDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = accoDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = accoDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = accoDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = accoDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = accoDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = accoDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        
                    };

                    accommodation.GoogleId = accoDetails["id"]?.ToString();
                    accommodation.UserAccommodationPreferences = combinedPreferences.Item1;
                    accommodation.UserAccommodationPreferences.Id = Guid.NewGuid();


                    //accommodation.UserAccommodationPreferences = new UserAccommodationPreferences
                    //{
                    //    UserId = null,
                    //    LuxuryHotelPreference = combinedPreferences.Item1.LuxuryHotelPreference,
                    //    BudgetHotelPreference = combinedPreferences.Item1.BudgetHotelPreference,
                    //    BoutiqueHotelPreference = combinedPreferences.Item1.BoutiqueHotelPreference,
                    //    HostelPreference = combinedPreferences.Item1.HostelPreference,
                    //    ApartmentPreference = combinedPreferences.Item1.ApartmentPreference,
                    //    ResortPreference = combinedPreferences.Item1.ResortPreference,
                    //    VillaPreference = combinedPreferences.Item1.VillaPreference,
                    //    GuestHousePreference = combinedPreferences.Item1.GuestHousePreference,
                    //    CampingPreference = combinedPreferences.Item1.CampingPreference,
                    //    GlampingPreference = combinedPreferences.Item1.GlampingPreference,
                    //    BedAndBreakfastPreference = combinedPreferences.Item1.BedAndBreakfastPreference,
                    //    AllInclusivePreference = combinedPreferences.Item1.AllInclusivePreference,
                    //    SpaAndWellnessPreference = combinedPreferences.Item1.SpaAndWellnessPreference,
                    //    PetFriendlyPreference = combinedPreferences.Item1.PetFriendlyPreference,
                    //    EcoFriendlyPreference = combinedPreferences.Item1.EcoFriendlyPreference,
                    //    RemoteLocationPreference = combinedPreferences.Item1.RemoteLocationPreference,
                    //    CityCenterPreference = combinedPreferences.Item1.CityCenterPreference,
                    //    FamilyFriendlyPreference = combinedPreferences.Item1.FamilyFriendlyPreference,
                    //    AdultsOnlyPreference = combinedPreferences.Item1.AdultsOnlyPreference,
                    //    HomestayPreference = combinedPreferences.Item1.HomestayPreference,
                    //    WaterfrontPreference = combinedPreferences.Item1.WaterfrontPreference,
                    //    HistoricalBuildingPreference = combinedPreferences.Item1.HistoricalBuildingPreference,
                    //    AirbnbPreference = combinedPreferences.Item1.AirbnbPreference,
                    //    CoLivingSpacePreference = combinedPreferences.Item1.CoLivingSpacePreference,
                    //    ExtendedStayPreference = combinedPreferences.Item1.ExtendedStayPreference,
                    //    Id = acco1pref
                    //};

                    travelDay.Accomodation = accommodation;
                }

                var breakfastDetails = await GetPlaceDetails(days[i].BreakfastPlaceName, destination);
                if (breakfastDetails != null)
                {
                    var break1Guid = Guid.NewGuid();
                    var breakfast = new Breakfast
                    {
                        Id = break1Guid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = breakfastDetails["id"]?.ToString(),
                        FormattedAddress = breakfastDetails["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = breakfastDetails["shortFormattedAddress"]?.ToString(),
                        Rating = breakfastDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = breakfastDetails["googleMapsUri"]?.ToString(),
                        WebsiteUri = breakfastDetails["websiteUri"]?.ToString(),
                        NationalPhoneNumber = breakfastDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = breakfastDetails["primaryType"]?.ToString(),
                        Reservable = breakfastDetails["reservable"]?.Value<bool>(),
                        ServesBrunch = breakfastDetails["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = breakfastDetails["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = breakfastDetails["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = breakfastDetails["liveMusic"]?.Value<bool>(),
                        MenuForChildren = breakfastDetails["menuForChildren"]?.Value<bool>(),
                        Restroom = breakfastDetails["restroom"]?.Value<bool>(),
                        GoodForGroups = breakfastDetails["goodForGroups"]?.Value<bool>(),
                        DisplayName = breakfastDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = breakfastDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = breakfastDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = breakfastDetails["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = breakfastDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = breakfastDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = breakfastDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = breakfastDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = breakfastDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = breakfastDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = breakfastDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = breakfastDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = breakfastDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = breakfastDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = breakfastDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = morningWeather,
                    };

                    var food1pref = new Guid();
                    breakfast.UserFoodPreferences = new UserFoodPreferences
                    {
                        UserId = null,
                        VeganPreference = combinedPreferences.Item2.VeganPreference,
                        VegetarianPreference = combinedPreferences.Item2.VegetarianPreference,
                        GlutenFreePreference = combinedPreferences.Item2.GlutenFreePreference,
                        HalalPreference = combinedPreferences.Item2.HalalPreference,
                        KosherPreference = combinedPreferences.Item2.KosherPreference,
                        SeafoodPreference = combinedPreferences.Item2.SeafoodPreference,
                        LocalCuisinePreference = combinedPreferences.Item2.LocalCuisinePreference,
                        FastFoodPreference = combinedPreferences.Item2.FastFoodPreference,
                        FinePreference = combinedPreferences.Item2.FinePreference,
                        StreetFoodPreference = combinedPreferences.Item2.StreetFoodPreference,
                        OrganicPreference = combinedPreferences.Item2.OrganicPreference,
                        BuffetPreference = combinedPreferences.Item2.BuffetPreference,
                        FoodTruckPreference = combinedPreferences.Item2.FoodTruckPreference,
                        CafeteriaPreference = combinedPreferences.Item2.CafeteriaPreference,
                        DeliveryPreference = combinedPreferences.Item2.DeliveryPreference,
                        AllergiesPreference = combinedPreferences.Item2.AllergiesPreference,
                        DairyFreePreference = combinedPreferences.Item2.DairyFreePreference,
                        NutFreePreference = combinedPreferences.Item2.NutFreePreference,
                        SpicyPreference = combinedPreferences.Item2.SpicyPreference,
                        SweetPreference = combinedPreferences.Item2.SweetPreference,
                        SaltyPreference = combinedPreferences.Item2.SaltyPreference,
                        SourPreference = combinedPreferences.Item2.SourPreference,
                        BitterPreference = combinedPreferences.Item2.BitterPreference,
                        UmamiPreference = combinedPreferences.Item2.UmamiPreference,
                        FusionPreference = combinedPreferences.Item2.FusionPreference,
                        Id = food1pref
                    };

                    travelDay.Breakfast = breakfast;
                }

                var lunchDetails = await GetPlaceDetails(days[i].LunchPlaceName, destination);
                if (lunchDetails != null)
                {
                    var lunch1Guid = new Guid();
                    var lunch = new Lunch
                    {
                        Id = lunch1Guid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = lunchDetails["id"]?.ToString(),
                        FormattedAddress = lunchDetails["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = lunchDetails["shortFormattedAddress"]?.ToString(),
                        Rating = lunchDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = lunchDetails["googleMapsUri"]?.ToString(),
                        WebsiteUri = lunchDetails["websiteUri"]?.ToString(),
                        NationalPhoneNumber = lunchDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = lunchDetails["primaryType"]?.ToString(),
                        Reservable = lunchDetails["reservable"]?.Value<bool>(),
                        ServesBrunch = lunchDetails["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = lunchDetails["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = lunchDetails["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = lunchDetails["liveMusic"]?.Value<bool>(),
                        MenuForChildren = lunchDetails["menuForChildren"]?.Value<bool>(),
                        Restroom = lunchDetails["restroom"]?.Value<bool>(),
                        GoodForGroups = lunchDetails["goodForGroups"]?.Value<bool>(),
                        ServesBeer = lunchDetails["servesBeer"]?.Value<bool>(),
                        ServesWine = lunchDetails["servesWine"]?.Value<bool>(),
                        DisplayName = lunchDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = lunchDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = lunchDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = lunchDetails["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = lunchDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = lunchDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = lunchDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = lunchDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = lunchDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = lunchDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = lunchDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = lunchDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = lunchDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = lunchDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = lunchDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = noonWeather
                    };

                    var food2Guid = new Guid();
                    lunch.UserFoodPreferences = new UserFoodPreferences
                    {
                        UserId = null,
                        VeganPreference = combinedPreferences.Item2.VeganPreference,
                        VegetarianPreference = combinedPreferences.Item2.VegetarianPreference,
                        GlutenFreePreference = combinedPreferences.Item2.GlutenFreePreference,
                        HalalPreference = combinedPreferences.Item2.HalalPreference,
                        KosherPreference = combinedPreferences.Item2.KosherPreference,
                        SeafoodPreference = combinedPreferences.Item2.SeafoodPreference,
                        LocalCuisinePreference = combinedPreferences.Item2.LocalCuisinePreference,
                        FastFoodPreference = combinedPreferences.Item2.FastFoodPreference,
                        FinePreference = combinedPreferences.Item2.FinePreference,
                        StreetFoodPreference = combinedPreferences.Item2.StreetFoodPreference,
                        OrganicPreference = combinedPreferences.Item2.OrganicPreference,
                        BuffetPreference = combinedPreferences.Item2.BuffetPreference,
                        FoodTruckPreference = combinedPreferences.Item2.FoodTruckPreference,
                        CafeteriaPreference = combinedPreferences.Item2.CafeteriaPreference,
                        DeliveryPreference = combinedPreferences.Item2.DeliveryPreference,
                        AllergiesPreference = combinedPreferences.Item2.AllergiesPreference,
                        DairyFreePreference = combinedPreferences.Item2.DairyFreePreference,
                        NutFreePreference = combinedPreferences.Item2.NutFreePreference,
                        SpicyPreference = combinedPreferences.Item2.SpicyPreference,
                        SweetPreference = combinedPreferences.Item2.SweetPreference,
                        SaltyPreference = combinedPreferences.Item2.SaltyPreference,
                        SourPreference = combinedPreferences.Item2.SourPreference,
                        BitterPreference = combinedPreferences.Item2.BitterPreference,
                        UmamiPreference = combinedPreferences.Item2.UmamiPreference,
                        FusionPreference = combinedPreferences.Item2.FusionPreference,
                        Id = food2Guid
                    };

                    travelDay.Lunch = lunch;
                }

                var dinnerDetails = await GetPlaceDetails(days[i].DinnerPlaceName, destination);
                if (dinnerDetails != null)
                {
                    var dinner1Guid = new Guid();
                    travelDay.Dinner = new Dinner
                    {
                        Id = dinner1Guid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = dinnerDetails["id"]?.ToString(),
                        FormattedAddress = dinnerDetails["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = dinnerDetails["shortFormattedAddress"]?.ToString(),
                        Rating = dinnerDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = dinnerDetails["googleMapsUri"]?.ToString(),
                        WebsiteUri = dinnerDetails["websiteUri"]?.ToString(),
                        NationalPhoneNumber = dinnerDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = dinnerDetails["primaryType"]?.ToString(),
                        Reservable = dinnerDetails["reservable"]?.Value<bool>(),
                        ServesBrunch = dinnerDetails["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = dinnerDetails["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = dinnerDetails["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = dinnerDetails["liveMusic"]?.Value<bool>(),
                        MenuForChildren = dinnerDetails["menuForChildren"]?.Value<bool>(),
                        Restroom = dinnerDetails["restroom"]?.Value<bool>(),
                        GoodForGroups = dinnerDetails["goodForGroups"]?.Value<bool>(),
                        ServesBeer = dinnerDetails["servesBeer"]?.Value<bool>(),
                        ServesWine = dinnerDetails["servesWine"]?.Value<bool>(),
                        DisplayName = dinnerDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = dinnerDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = dinnerDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = dinnerDetails["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = dinnerDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = dinnerDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = dinnerDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = dinnerDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = dinnerDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = dinnerDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = dinnerDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = dinnerDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = dinnerDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = dinnerDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = dinnerDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = eveningWeather,
                    };

                    var food2prefGuid = new Guid();
                    travelDay.Dinner.UserFoodPreferences = new UserFoodPreferences
                    {
                        UserId = null,
                        VeganPreference = combinedPreferences.Item2.VeganPreference,
                        VegetarianPreference = combinedPreferences.Item2.VegetarianPreference,
                        GlutenFreePreference = combinedPreferences.Item2.GlutenFreePreference,
                        HalalPreference = combinedPreferences.Item2.HalalPreference,
                        KosherPreference = combinedPreferences.Item2.KosherPreference,
                        SeafoodPreference = combinedPreferences.Item2.SeafoodPreference,
                        LocalCuisinePreference = combinedPreferences.Item2.LocalCuisinePreference,
                        FastFoodPreference = combinedPreferences.Item2.FastFoodPreference,
                        FinePreference = combinedPreferences.Item2.FinePreference,
                        StreetFoodPreference = combinedPreferences.Item2.StreetFoodPreference,
                        OrganicPreference = combinedPreferences.Item2.OrganicPreference,
                        BuffetPreference = combinedPreferences.Item2.BuffetPreference,
                        FoodTruckPreference = combinedPreferences.Item2.FoodTruckPreference,
                        CafeteriaPreference = combinedPreferences.Item2.CafeteriaPreference,
                        DeliveryPreference = combinedPreferences.Item2.DeliveryPreference,
                        AllergiesPreference = combinedPreferences.Item2.AllergiesPreference,
                        DairyFreePreference = combinedPreferences.Item2.DairyFreePreference,
                        NutFreePreference = combinedPreferences.Item2.NutFreePreference,
                        SpicyPreference = combinedPreferences.Item2.SpicyPreference,
                        SweetPreference = combinedPreferences.Item2.SweetPreference,
                        SaltyPreference = combinedPreferences.Item2.SaltyPreference,
                        SourPreference = combinedPreferences.Item2.SourPreference,
                        BitterPreference = combinedPreferences.Item2.BitterPreference,
                        UmamiPreference = combinedPreferences.Item2.UmamiPreference,
                        FusionPreference = combinedPreferences.Item2.FusionPreference,
                        Id = food2prefGuid
                    };




                }

                var firstPlaceDetails = await GetPlaceDetails(days[i].FirstPlaceName, destination);
                if (firstPlaceDetails != null)
                {
                    var firstPlaceGuid = new Guid();
                    var firstPlace = new Domain.Entities.Route.Place
                    {
                        Id = firstPlaceGuid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = firstPlaceDetails["id"]?.ToString(),
                        FormattedAddress = firstPlaceDetails["formattedAddress"]?.ToString(),
                        Rating = firstPlaceDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = firstPlaceDetails["googleMapsUri"]?.ToString(),
                        WebsiteUri = firstPlaceDetails["websiteUri"]?.ToString(),
                        NationalPhoneNumber = firstPlaceDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = firstPlaceDetails["primaryType"]?.ToString(),
                        GoodForChildren = firstPlaceDetails["goodForChildren"]?.Value<bool>(),
                        Restroom = firstPlaceDetails["restroom"]?.Value<bool>(),
                        Location = firstPlaceDetails["location"] != null ? new Domain.Entities.Route.Location
                        {
                            Latitude = firstPlaceDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = firstPlaceDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = firstPlaceDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = firstPlaceDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = firstPlaceDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = firstPlaceDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        DisplayName = firstPlaceDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = firstPlaceDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = firstPlaceDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        PaymentOptions = firstPlaceDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = firstPlaceDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = firstPlaceDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = firstPlaceDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = firstPlaceDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = morningWeather,
                    };

                    var fppersonGuid = new Guid();
                    firstPlace.UserPersonalization = new UserPersonalization
                    {
                        UserId = null,
                        AdventurePreference = combinedPreferences.Item3.AdventurePreference,
                        RelaxationPreference = combinedPreferences.Item3.RelaxationPreference,
                        CulturalPreference = combinedPreferences.Item3.CulturalPreference,
                        NaturePreference = combinedPreferences.Item3.NaturePreference,
                        UrbanPreference = combinedPreferences.Item3.UrbanPreference,
                        RuralPreference = combinedPreferences.Item3.RuralPreference,
                        LuxuryPreference = combinedPreferences.Item3.LuxuryPreference,
                        BudgetPreference = combinedPreferences.Item3.BudgetPreference,
                        SoloTravelPreference = combinedPreferences.Item3.SoloTravelPreference,
                        GroupTravelPreference = combinedPreferences.Item3.GroupTravelPreference,
                        FamilyTravelPreference = combinedPreferences.Item3.FamilyTravelPreference,
                        CoupleTravelPreference = combinedPreferences.Item3.CoupleTravelPreference,
                        BeachPreference = combinedPreferences.Item3.BeachPreference,
                        MountainPreference = combinedPreferences.Item3.MountainPreference,
                        DesertPreference = combinedPreferences.Item3.DesertPreference,
                        ForestPreference = combinedPreferences.Item3.ForestPreference,
                        IslandPreference = combinedPreferences.Item3.IslandPreference,
                        LakePreference = combinedPreferences.Item3.LakePreference,
                        RiverPreference = combinedPreferences.Item3.RiverPreference,
                        WaterfallPreference = combinedPreferences.Item3.WaterfallPreference,
                        CavePreference = combinedPreferences.Item3.CavePreference,
                        VolcanoPreference = combinedPreferences.Item3.VolcanoPreference,
                        GlacierPreference = combinedPreferences.Item3.GlacierPreference,
                        CanyonPreference = combinedPreferences.Item3.CanyonPreference,
                        ValleyPreference = combinedPreferences.Item3.ValleyPreference,
                        Id = fppersonGuid
                    };

                    travelDay.FirstPlace = firstPlace;
                }

                var secondPlaceDetails = await GetPlaceDetails(days[i].SecondPlaceName, destination);
                if (secondPlaceDetails != null)
                {
                    var secondGuid = new Guid();
                    travelDay.SecondPlace = new Domain.Entities.Route.Place
                    {
                        Id = secondGuid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = secondPlaceDetails["id"]?.ToString(),
                        FormattedAddress = secondPlaceDetails["formattedAddress"]?.ToString(),
                        Rating = secondPlaceDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = secondPlaceDetails["googleMapsUri"]?.ToString(),
                        WebsiteUri = secondPlaceDetails["websiteUri"]?.ToString(),
                        NationalPhoneNumber = secondPlaceDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = secondPlaceDetails["primaryType"]?.ToString(),
                        GoodForChildren = secondPlaceDetails["goodForChildren"]?.Value<bool>(),
                        Restroom = secondPlaceDetails["restroom"]?.Value<bool>(),
                        Location = secondPlaceDetails["location"] != null ? new Domain.Entities.Route.Location
                        {
                            Latitude = secondPlaceDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = secondPlaceDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = secondPlaceDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = secondPlaceDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = secondPlaceDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = secondPlaceDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        DisplayName = secondPlaceDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = secondPlaceDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = secondPlaceDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        PaymentOptions = secondPlaceDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = secondPlaceDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = secondPlaceDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = secondPlaceDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = secondPlaceDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = noonWeather,
                    };

                    var spPersonalizationGuid = new Guid();
                    travelDay.SecondPlace.UserPersonalization = new UserPersonalization
                    {
                        UserId = null,
                        AdventurePreference = combinedPreferences.Item3.AdventurePreference,
                        RelaxationPreference = combinedPreferences.Item3.RelaxationPreference,
                        CulturalPreference = combinedPreferences.Item3.CulturalPreference,
                        NaturePreference = combinedPreferences.Item3.NaturePreference,
                        UrbanPreference = combinedPreferences.Item3.UrbanPreference,
                        RuralPreference = combinedPreferences.Item3.RuralPreference,
                        LuxuryPreference = combinedPreferences.Item3.LuxuryPreference,
                        BudgetPreference = combinedPreferences.Item3.BudgetPreference,
                        SoloTravelPreference = combinedPreferences.Item3.SoloTravelPreference,
                        GroupTravelPreference = combinedPreferences.Item3.GroupTravelPreference,
                        FamilyTravelPreference = combinedPreferences.Item3.FamilyTravelPreference,
                        CoupleTravelPreference = combinedPreferences.Item3.CoupleTravelPreference,
                        BeachPreference = combinedPreferences.Item3.BeachPreference,
                        MountainPreference = combinedPreferences.Item3.MountainPreference,
                        DesertPreference = combinedPreferences.Item3.DesertPreference,
                        ForestPreference = combinedPreferences.Item3.ForestPreference,
                        IslandPreference = combinedPreferences.Item3.IslandPreference,
                        LakePreference = combinedPreferences.Item3.LakePreference,
                        RiverPreference = combinedPreferences.Item3.RiverPreference,
                        WaterfallPreference = combinedPreferences.Item3.WaterfallPreference,
                        CavePreference = combinedPreferences.Item3.CavePreference,
                        VolcanoPreference = combinedPreferences.Item3.VolcanoPreference,
                        GlacierPreference = combinedPreferences.Item3.GlacierPreference,
                        CanyonPreference = combinedPreferences.Item3.CanyonPreference,
                        ValleyPreference = combinedPreferences.Item3.ValleyPreference,
                        Id = spPersonalizationGuid
                    };

                }

                var thirdPlaceDetails = await GetPlaceDetails(days[i].ThirdPlaceName, destination);
                if (thirdPlaceDetails != null)
                {
                    var tpGuid = new Guid();
                    travelDay.ThirdPlace = new Domain.Entities.Route.Place
                    {
                        Id = tpGuid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = thirdPlaceDetails["id"]?.ToString(),
                        FormattedAddress = thirdPlaceDetails["formattedAddress"]?.ToString(),
                        Rating = thirdPlaceDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = thirdPlaceDetails["googleMapsUri"]?.ToString(),
                        WebsiteUri = thirdPlaceDetails["websiteUri"]?.ToString(),
                        NationalPhoneNumber = thirdPlaceDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = thirdPlaceDetails["primaryType"]?.ToString(),
                        GoodForChildren = thirdPlaceDetails["goodForChildren"]?.Value<bool>(),
                        Restroom = thirdPlaceDetails["restroom"]?.Value<bool>(),
                        Location = thirdPlaceDetails["location"] != null ? new Domain.Entities.Route.Location
                        {
                            Latitude = thirdPlaceDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = thirdPlaceDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = thirdPlaceDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = thirdPlaceDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = thirdPlaceDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = thirdPlaceDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        DisplayName = thirdPlaceDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = thirdPlaceDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = thirdPlaceDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        PaymentOptions = thirdPlaceDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = thirdPlaceDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = thirdPlaceDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = thirdPlaceDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = thirdPlaceDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = noonWeather
                    };

                    var tpPersonalizationGuid = new Guid();
                    travelDay.ThirdPlace.UserPersonalization = new UserPersonalization
                    {
                        UserId = null,
                        AdventurePreference = combinedPreferences.Item3.AdventurePreference,
                        RelaxationPreference = combinedPreferences.Item3.RelaxationPreference,
                        CulturalPreference = combinedPreferences.Item3.CulturalPreference,
                        NaturePreference = combinedPreferences.Item3.NaturePreference,
                        UrbanPreference = combinedPreferences.Item3.UrbanPreference,
                        RuralPreference = combinedPreferences.Item3.RuralPreference,
                        LuxuryPreference = combinedPreferences.Item3.LuxuryPreference,
                        BudgetPreference = combinedPreferences.Item3.BudgetPreference,
                        SoloTravelPreference = combinedPreferences.Item3.SoloTravelPreference,
                        GroupTravelPreference = combinedPreferences.Item3.GroupTravelPreference,
                        FamilyTravelPreference = combinedPreferences.Item3.FamilyTravelPreference,
                        CoupleTravelPreference = combinedPreferences.Item3.CoupleTravelPreference,
                        BeachPreference = combinedPreferences.Item3.BeachPreference,
                        MountainPreference = combinedPreferences.Item3.MountainPreference,
                        DesertPreference = combinedPreferences.Item3.DesertPreference,
                        ForestPreference = combinedPreferences.Item3.ForestPreference,
                        IslandPreference = combinedPreferences.Item3.IslandPreference,
                        LakePreference = combinedPreferences.Item3.LakePreference,
                        RiverPreference = combinedPreferences.Item3.RiverPreference,
                        WaterfallPreference = combinedPreferences.Item3.WaterfallPreference,
                        CavePreference = combinedPreferences.Item3.CavePreference,
                        VolcanoPreference = combinedPreferences.Item3.VolcanoPreference,
                        GlacierPreference = combinedPreferences.Item3.GlacierPreference,
                        CanyonPreference = combinedPreferences.Item3.CanyonPreference,
                        ValleyPreference = combinedPreferences.Item3.ValleyPreference,
                        Id = tpPersonalizationGuid
                    };

                }

                var afterDinnerDetails = await GetPlaceDetails(days[i].AfterDinnerPlaceName, destination);
                if (afterDinnerDetails != null)
                {
                    var adGuid = Guid.NewGuid();
                    travelDay.PlaceAfterDinner = new PlaceAfterDinner
                    {
                        Id = adGuid,
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = afterDinnerDetails["id"]?.ToString(),
                        FormattedAddress = afterDinnerDetails["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = afterDinnerDetails["shortFormattedAddress"]?.ToString(),
                        Rating = afterDinnerDetails["rating"]?.Value<double>(),
                        GoogleMapsUri = afterDinnerDetails["googleMapsUri"]?.ToString(),
                        WebsiteUri = afterDinnerDetails["websiteUri"]?.ToString(),
                        NationalPhoneNumber = afterDinnerDetails["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = afterDinnerDetails["primaryType"]?.ToString(),
                        Reservable = afterDinnerDetails["reservable"]?.Value<bool>(),
                        ServesBrunch = afterDinnerDetails["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = afterDinnerDetails["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = afterDinnerDetails["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = afterDinnerDetails["liveMusic"]?.Value<bool>(),
                        MenuForChildren = afterDinnerDetails["menuForChildren"]?.Value<bool>(),
                        Restroom = afterDinnerDetails["restroom"]?.Value<bool>(),
                        GoodForGroups = afterDinnerDetails["goodForGroups"]?.Value<bool>(),
                        GoodForChildren = afterDinnerDetails["goodForChildren"]?.Value<bool>(),
                        Takeout = afterDinnerDetails["takeout"]?.Value<bool>(),
                        Delivery = afterDinnerDetails["delivery"]?.Value<bool>(),
                        CurbsidePickup = afterDinnerDetails["curbsidePickup"]?.Value<bool>(),
                        ServesBeer = afterDinnerDetails["servesBeer"]?.Value<bool>(),
                        ServesWine = afterDinnerDetails["servesWine"]?.Value<bool>(),
                        ServesCocktails = afterDinnerDetails["servesCocktails"]?.Value<bool>(),
                        DisplayName = afterDinnerDetails["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = afterDinnerDetails["displayName"]?["text"]?.ToString(),
                            LangugageCode = afterDinnerDetails["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = afterDinnerDetails["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = afterDinnerDetails["location"]?["latitude"]?.Value<double>(),
                            Longitude = afterDinnerDetails["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = afterDinnerDetails["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = afterDinnerDetails["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = afterDinnerDetails["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = afterDinnerDetails["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = afterDinnerDetails["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = afterDinnerDetails["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = afterDinnerDetails["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = afterDinnerDetails["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = afterDinnerDetails["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = eveningWeather,
                    };
                    var pdfoodpref = new Guid();
                    travelDay.PlaceAfterDinner.UserFoodPreferences = new UserFoodPreferences
                    {
                        UserId = null,
                        VeganPreference = combinedPreferences.Item2.VeganPreference,
                        VegetarianPreference = combinedPreferences.Item2.VegetarianPreference,
                        GlutenFreePreference = combinedPreferences.Item2.GlutenFreePreference,
                        HalalPreference = combinedPreferences.Item2.HalalPreference,
                        KosherPreference = combinedPreferences.Item2.KosherPreference,
                        SeafoodPreference = combinedPreferences.Item2.SeafoodPreference,
                        LocalCuisinePreference = combinedPreferences.Item2.LocalCuisinePreference,
                        FastFoodPreference = combinedPreferences.Item2.FastFoodPreference,
                        FinePreference = combinedPreferences.Item2.FinePreference,
                        StreetFoodPreference = combinedPreferences.Item2.StreetFoodPreference,
                        OrganicPreference = combinedPreferences.Item2.OrganicPreference,
                        BuffetPreference = combinedPreferences.Item2.BuffetPreference,
                        FoodTruckPreference = combinedPreferences.Item2.FoodTruckPreference,
                        CafeteriaPreference = combinedPreferences.Item2.CafeteriaPreference,
                        DeliveryPreference = combinedPreferences.Item2.DeliveryPreference,
                        AllergiesPreference = combinedPreferences.Item2.AllergiesPreference,
                        DairyFreePreference = combinedPreferences.Item2.DairyFreePreference,
                        NutFreePreference = combinedPreferences.Item2.NutFreePreference,
                        SpicyPreference = combinedPreferences.Item2.SpicyPreference,
                        SweetPreference = combinedPreferences.Item2.SweetPreference,
                        SaltyPreference = combinedPreferences.Item2.SaltyPreference,
                        SourPreference = combinedPreferences.Item2.SourPreference,
                        BitterPreference = combinedPreferences.Item2.BitterPreference,
                        UmamiPreference = combinedPreferences.Item2.UmamiPreference,
                        FusionPreference = combinedPreferences.Item2.FusionPreference,
                        Id = pdfoodpref
                    };

                }

                standardRoute.TravelDays.Add(travelDay);
            }



            await _context.StandardRoutes.AddAsync(standardRoute);
            await _context.SaveChangesAsync();

            var routeDto = new StandardRoute
            {
                Id = standardRoute.Id,
                status = standardRoute.status,
                City = standardRoute.City,
                Days = standardRoute.Days,
                name = standardRoute.name,
                UserId = standardRoute.UserId,
                LikeCount = standardRoute.LikeCount,
                TravelDays = standardRoute.TravelDays.Select(day => new TravelDay
                {
                    Id = day.Id,
                    DayDescription = day.DayDescription,
                    approxPrice = day.approxPrice,
                    Accomodation = new TravelAccomodation
                    {
                        Id = day.Accomodation?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.Accomodation?._PRICE_LEVEL,
                        Star = day.Accomodation?.Star,
                        GoogleId = day.Accomodation?.GoogleId,
                        FormattedAddress = day.Accomodation?.FormattedAddress,
                        Rating = day.Accomodation?.Rating,
                        GoogleMapsUri = day.Accomodation?.GoogleMapsUri,
                        GoodForChildren = day.Accomodation?.GoodForChildren,
                        Restroom = day.Accomodation?.Restroom,
                        WebsiteUri = day.Accomodation?.WebsiteUri,
                        InternationalPhoneNumber = day.Accomodation?.InternationalPhoneNumber,
                        NationalPhoneNumber = day.Accomodation?.NationalPhoneNumber,
                        PrimaryType = day.Accomodation?.PrimaryType,
                        DisplayName = day.Accomodation?.DisplayName,
                        Location = day.Accomodation?.Location,
                        Photos = day.Accomodation?.Photos?.ToList(),
                        RegularOpeningHours = day.Accomodation?.RegularOpeningHours,
                        PaymentOptions = day.Accomodation?.PaymentOptions,
                        //UserAccommodationPreferences = day.Accomodation?.UserAccommodationPreferences

                    },
                    Breakfast = new Breakfast
                    {
                        Id = day.Breakfast?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.Breakfast?._PRICE_LEVEL,
                        GoogleId = day.Breakfast?.GoogleId,
                        FormattedAddress = day.Breakfast?.FormattedAddress,
                        ShortFormattedAddress = day.Breakfast?.ShortFormattedAddress,
                        Rating = day.Breakfast?.Rating,
                        GoogleMapsUri = day.Breakfast?.GoogleMapsUri,
                        WebsiteUri = day.Breakfast?.WebsiteUri,
                        NationalPhoneNumber = day.Breakfast?.NationalPhoneNumber,
                        PrimaryType = day.Breakfast?.PrimaryType,
                        Reservable = day.Breakfast?.Reservable,
                        ServesBrunch = day.Breakfast?.ServesBrunch,
                        ServesVegetarianFood = day.Breakfast?.ServesVegetarianFood,
                        OutdoorSeating = day.Breakfast?.OutdoorSeating,
                        LiveMusic = day.Breakfast?.LiveMusic,
                        MenuForChildren = day.Breakfast?.MenuForChildren,
                        Restroom = day.Breakfast?.Restroom,
                        GoodForGroups = day.Breakfast?.GoodForGroups,
                        DisplayName = day.Breakfast?.DisplayName,
                        Location = day.Breakfast?.Location,
                        Photos = day.Breakfast?.Photos?.ToList(),
                        RegularOpeningHours = day.Breakfast?.RegularOpeningHours,
                        PaymentOptions = day.Breakfast?.PaymentOptions,
                        Weather = day.Breakfast?.Weather,
                        //UserFoodPreferences = day.Breakfast?.UserFoodPreferences
                    },
                    Lunch = new Lunch
                    {
                        Id = day.Lunch?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.Lunch?._PRICE_LEVEL,
                        GoogleId = day.Lunch?.GoogleId,
                        FormattedAddress = day.Lunch?.FormattedAddress,
                        ShortFormattedAddress = day.Lunch?.ShortFormattedAddress,
                        Rating = day.Lunch?.Rating,
                        GoogleMapsUri = day.Lunch?.GoogleMapsUri,
                        WebsiteUri = day.Lunch?.WebsiteUri,
                        NationalPhoneNumber = day.Lunch?.NationalPhoneNumber,
                        PrimaryType = day.Lunch?.PrimaryType,
                        Reservable = day.Lunch?.Reservable,
                        ServesBrunch = day.Lunch?.ServesBrunch,
                        ServesVegetarianFood = day.Lunch?.ServesVegetarianFood,
                        OutdoorSeating = day.Lunch?.OutdoorSeating,
                        LiveMusic = day.Lunch?.LiveMusic,
                        MenuForChildren = day.Lunch?.MenuForChildren,
                        Restroom = day.Lunch?.Restroom,
                        GoodForGroups = day.Lunch?.GoodForGroups,
                        ServesBeer = day.Lunch?.ServesBeer,
                        ServesWine = day.Lunch?.ServesWine,
                        DisplayName = day.Lunch?.DisplayName,
                        Location = day.Lunch?.Location,
                        Photos = day.Lunch?.Photos?.ToList(),
                        RegularOpeningHours = day.Lunch?.RegularOpeningHours,
                        PaymentOptions = day.Lunch?.PaymentOptions,
                        Weather = day.Lunch?.Weather,
                        //UserFoodPreferences = day.Lunch?.UserFoodPreferences
                    },
                    Dinner = new Dinner
                    {
                        Id = day.Dinner?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.Dinner?._PRICE_LEVEL,
                        GoogleId = day.Dinner?.GoogleId,
                        FormattedAddress = day.Dinner?.FormattedAddress,
                        ShortFormattedAddress = day.Dinner?.ShortFormattedAddress,
                        Rating = day.Dinner?.Rating,
                        GoogleMapsUri = day.Dinner?.GoogleMapsUri,
                        WebsiteUri = day.Dinner?.WebsiteUri,
                        NationalPhoneNumber = day.Dinner?.NationalPhoneNumber,
                        PrimaryType = day.Dinner?.PrimaryType,
                        Reservable = day.Dinner?.Reservable,
                        ServesBrunch = day.Dinner?.ServesBrunch,
                        ServesVegetarianFood = day.Dinner?.ServesVegetarianFood,
                        OutdoorSeating = day.Dinner?.OutdoorSeating,
                        LiveMusic = day.Dinner?.LiveMusic,
                        MenuForChildren = day.Dinner?.MenuForChildren,
                        Restroom = day.Dinner?.Restroom,
                        GoodForGroups = day.Dinner?.GoodForGroups,
                        ServesBeer = day.Dinner?.ServesBeer,
                        ServesWine = day.Dinner?.ServesWine,
                        DisplayName = day.Dinner?.DisplayName,
                        Location = day.Dinner?.Location,
                        Photos = day.Dinner?.Photos?.ToList(),
                        RegularOpeningHours = day.Dinner?.RegularOpeningHours,
                        PaymentOptions = day.Dinner?.PaymentOptions,
                        Weather = day.Dinner?.Weather,
                        //UserFoodPreferences = day.Dinner?.UserFoodPreferences
                    },
                    FirstPlace = new Domain.Entities.Route.Place
                    {
                        Id = day.FirstPlace?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.FirstPlace?._PRICE_LEVEL,
                        GoogleId = day.FirstPlace?.GoogleId,
                        FormattedAddress = day.FirstPlace?.FormattedAddress,
                        Rating = day.FirstPlace?.Rating,
                        GoogleMapsUri = day.FirstPlace?.GoogleMapsUri,
                        WebsiteUri = day.FirstPlace?.WebsiteUri,
                        NationalPhoneNumber = day.FirstPlace?.NationalPhoneNumber,
                        PrimaryType = day.FirstPlace?.PrimaryType,
                        GoodForChildren = day.FirstPlace?.GoodForChildren,
                        Restroom = day.FirstPlace?.Restroom,
                        Location = day.FirstPlace?.Location,
                        RegularOpeningHours = day.FirstPlace?.RegularOpeningHours,
                        DisplayName = day.FirstPlace?.DisplayName,
                        PaymentOptions = day.FirstPlace?.PaymentOptions,
                        Photos = day.FirstPlace?.Photos?.ToList(),
                        Weather = day.FirstPlace?.Weather,
                        //UserPersonalization = day.FirstPlace?.UserPersonalization
                    },
                    SecondPlace = new Domain.Entities.Route.Place
                    {
                        Id = day.SecondPlace?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.SecondPlace?._PRICE_LEVEL,
                        GoogleId = day.SecondPlace?.GoogleId,
                        FormattedAddress = day.SecondPlace?.FormattedAddress,
                        Rating = day.SecondPlace?.Rating,
                        GoogleMapsUri = day.SecondPlace?.GoogleMapsUri,
                        WebsiteUri = day.SecondPlace?.WebsiteUri,
                        NationalPhoneNumber = day.SecondPlace?.NationalPhoneNumber,
                        PrimaryType = day.SecondPlace?.PrimaryType,
                        GoodForChildren = day.SecondPlace?.GoodForChildren,
                        Restroom = day.SecondPlace?.Restroom,
                        Location = day.SecondPlace?.Location,
                        RegularOpeningHours = day.SecondPlace?.RegularOpeningHours,
                        DisplayName = day.SecondPlace?.DisplayName,
                        PaymentOptions = day.SecondPlace?.PaymentOptions,
                        Photos = day.SecondPlace?.Photos?.ToList(),
                        Weather = day.SecondPlace?.Weather,
                        //UserPersonalization = day.SecondPlace?.UserPersonalization
                    },
                    ThirdPlace = new Domain.Entities.Route.Place
                    {
                        Id = day.ThirdPlace?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.ThirdPlace?._PRICE_LEVEL,
                        GoogleId = day.ThirdPlace?.GoogleId,
                        FormattedAddress = day.ThirdPlace?.FormattedAddress,
                        Rating = day.ThirdPlace?.Rating,
                        GoogleMapsUri = day.ThirdPlace?.GoogleMapsUri,
                        WebsiteUri = day.ThirdPlace?.WebsiteUri,
                        NationalPhoneNumber = day.ThirdPlace?.NationalPhoneNumber,
                        PrimaryType = day.ThirdPlace?.PrimaryType,
                        GoodForChildren = day.ThirdPlace?.GoodForChildren,
                        Restroom = day.ThirdPlace?.Restroom,
                        Location = day.ThirdPlace?.Location,
                        RegularOpeningHours = day.ThirdPlace?.RegularOpeningHours,
                        DisplayName = day.ThirdPlace?.DisplayName,
                        PaymentOptions = day.ThirdPlace?.PaymentOptions,
                        Photos = day.ThirdPlace?.Photos?.ToList(),
                        Weather = day.ThirdPlace?.Weather,
                        //UserPersonalization = day.ThirdPlace?.UserPersonalization
                    },
                    PlaceAfterDinner = new PlaceAfterDinner
                    {
                        Id = day.PlaceAfterDinner?.Id ?? Guid.Empty,
                        _PRICE_LEVEL = day.PlaceAfterDinner?._PRICE_LEVEL,
                        GoogleId = day.PlaceAfterDinner?.GoogleId,
                        FormattedAddress = day.PlaceAfterDinner?.FormattedAddress,
                        ShortFormattedAddress = day.PlaceAfterDinner?.ShortFormattedAddress,
                        Rating = day.PlaceAfterDinner?.Rating,
                        GoogleMapsUri = day.PlaceAfterDinner?.GoogleMapsUri,
                        WebsiteUri = day.PlaceAfterDinner?.WebsiteUri,
                        NationalPhoneNumber = day.PlaceAfterDinner?.NationalPhoneNumber,
                        PrimaryType = day.PlaceAfterDinner?.PrimaryType,
                        Reservable = day.PlaceAfterDinner?.Reservable,
                        ServesBrunch = day.PlaceAfterDinner?.ServesBrunch,
                        ServesVegetarianFood = day.PlaceAfterDinner?.ServesVegetarianFood,
                        OutdoorSeating = day.PlaceAfterDinner?.OutdoorSeating,
                        LiveMusic = day.PlaceAfterDinner?.LiveMusic,
                        MenuForChildren = day.PlaceAfterDinner?.MenuForChildren,
                        Restroom = day.PlaceAfterDinner?.Restroom,
                        GoodForGroups = day.PlaceAfterDinner?.GoodForGroups,
                        GoodForChildren = day.PlaceAfterDinner?.GoodForChildren,
                        Takeout = day.PlaceAfterDinner?.Takeout,
                        Delivery = day.PlaceAfterDinner?.Delivery,
                        CurbsidePickup = day.PlaceAfterDinner?.CurbsidePickup,
                        ServesBeer = day.PlaceAfterDinner?.ServesBeer,
                        ServesWine = day.PlaceAfterDinner?.ServesWine,
                        ServesCocktails = day.PlaceAfterDinner?.ServesCocktails,
                        DisplayName = day.PlaceAfterDinner?.DisplayName,
                        Location = day.PlaceAfterDinner?.Location,
                        Photos = day.PlaceAfterDinner?.Photos?.ToList(),
                        RegularOpeningHours = day.PlaceAfterDinner?.RegularOpeningHours,
                        PaymentOptions = day.PlaceAfterDinner?.PaymentOptions,
                        Weather = day.PlaceAfterDinner?.Weather,
                        //UserPersonalization = day.PlaceAfterDinner?.UserPersonalization
                    }
                }).ToList()
            };

            return new CreateCustomRouteCommandResponseBody { Route = routeDto };
        }



        public async Task<CreateCustomRouteCommandResponseBody> CreateCustomRouteWithPreferences(string? destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL,string? username,List<string>? usernames)
        {
            destination = char.ToUpper(destination[0]) + destination[1..].ToLowerInvariant();
            var dayCount = (endDate.Value.DayNumber - startDate.Value.DayNumber);
            if (dayCount == 0) dayCount = 1;

            var allUserPreferences = new List<(UserAccommodationPreferences Accommodation,
                                     UserFoodPreferences Food,
                                     UserPersonalization Personal)>();

            AppUser? headuser = await _userManager.FindByNameAsync(username);
            if (headuser == null)
                throw new Application.Exceptions.Login.UserNotFoundException("Head user not found", 7);

            var headUserAccommodationPrefs = await _context.UserAccommodationPreferences
                .FirstOrDefaultAsync(p => p.UserId == headuser.Id);
            var headUserFoodPrefs = await _context.UserFoodPreferences
                .FirstOrDefaultAsync(p => p.UserId == headuser.Id);
            var headUserPersonalization = await _context.UserPersonalizations
                .FirstOrDefaultAsync(p => p.UserId == headuser.Id);

            if (headUserAccommodationPrefs != null && headUserFoodPrefs != null && headUserPersonalization != null)
            {
                allUserPreferences.Add((
                    headUserAccommodationPrefs,
                    headUserFoodPrefs,
                    headUserPersonalization
                ));
            }

            if (usernames != null)
            {
                foreach (var otherUsername in usernames)
                {
                    var otherUser = await _userManager.FindByNameAsync(otherUsername);
                    if (otherUser == null)
                        throw new Application.Exceptions.Login.UserNotFoundException($"User {otherUsername} not found", 7);

                    var userAccommodationPrefs = await _context.UserAccommodationPreferences
                        .FirstOrDefaultAsync(p => p.UserId == otherUser.Id);
                    var userFoodPrefs = await _context.UserFoodPreferences
                        .FirstOrDefaultAsync(p => p.UserId == otherUser.Id);
                    var userPersonalization = await _context.UserPersonalizations
                        .FirstOrDefaultAsync(p => p.UserId == otherUser.Id);

                    if (userAccommodationPrefs != null && userFoodPrefs != null && userPersonalization != null)
                    {
                        allUserPreferences.Add((
                            userAccommodationPrefs,
                            userFoodPrefs,
                            userPersonalization
                        ));
                    }
                }

                var result = CombineUserPreferencesForCustoms(allUserPreferences, dayCount);

                var response = await _geminiAIService.CreateCustomRoute(destination, dayCount, startDate, endDate, _PRICE_LEVEL , result.Item1, result.Item2, result.Item3);

            }
            return null;
        }


        private (UserAccommodationPreferences, UserFoodPreferences, UserPersonalization) CombineUserPreferencesForCustoms(List<(UserAccommodationPreferences Accommodation, UserFoodPreferences Food, UserPersonalization Personal)> preferences, int dayCount)
        {
            var random = new Random();
            var combinedAccommodation = new UserAccommodationPreferences();
            var combinedFood = new UserFoodPreferences();
            var combinedPersonal = new UserPersonalization();

            bool hasVegan = preferences.Any(p => p.Food.VeganPreference > 50);
            bool hasVegetarian = preferences.Any(p => p.Food.VegetarianPreference > 50);
            bool hasHalal = preferences.Any(p => p.Food.HalalPreference > 50);
            bool hasKosher = preferences.Any(p => p.Food.KosherPreference > 50);
            bool hasGlutenFree = preferences.Any(p => p.Food.GlutenFreePreference > 50);
            bool hasDairyFree = preferences.Any(p => p.Food.DairyFreePreference > 50);
            bool hasNutAllergy = preferences.Any(p => p.Food.NutFreePreference > 50);

            var accommodationPreferences = new Dictionary<string, double>();
            foreach (var prop in typeof(UserAccommodationPreferences).GetProperties())
            {
                if (prop.PropertyType == typeof(int?) && prop.Name.EndsWith("Preference"))
                {
                    var values = preferences
                        .Select(p => (int?)prop.GetValue(p.Accommodation))
                        .Where(v => v.HasValue)
                        .Select(v => v.Value);

                    if (values.Any())
                    {
                        accommodationPreferences[prop.Name] = values.Average();
                    }
                }
            }

            var totalAccWeight = accommodationPreferences.Values.Sum();
            var randomAccNumber = random.NextDouble() * totalAccWeight;
            var currentAccWeight = 0.0;
            
            foreach (var prop in typeof(UserAccommodationPreferences).GetProperties())
            {
                if (prop.PropertyType == typeof(int?) && prop.Name.EndsWith("Preference"))
                {
                    prop.SetValue(combinedAccommodation, 0);
                }
            }

            foreach (var acc in accommodationPreferences)
            {
                currentAccWeight += acc.Value;
                if (randomAccNumber <= currentAccWeight)
                {
                    var prop = typeof(UserAccommodationPreferences).GetProperty(acc.Key);
                    prop?.SetValue(combinedAccommodation, 100);
                    break;
                }
            }

            var foodPreferences = new Dictionary<string, double>();
            foreach (var prop in typeof(UserFoodPreferences).GetProperties())
            {
                if (prop.PropertyType == typeof(int?) && prop.Name.EndsWith("Preference"))
                {
                    var values = preferences
                        .Select(p => (int?)prop.GetValue(p.Food))
                        .Where(v => v.HasValue)
                        .Select(v => v.Value);

                    if (values.Any())
                    {
                        foodPreferences[prop.Name] = values.Average();
                    }
                }
            }

            foreach (var prop in typeof(UserFoodPreferences).GetProperties())
            {
                if (prop.PropertyType == typeof(int?))
                {
                    prop.SetValue(combinedFood, 0); 
                }
            }

           
            if (hasVegan)
            {
                combinedFood.VeganPreference = 100;
                combinedFood.VegetarianPreference = 100;
            }
            else if (hasVegetarian)
            {
                combinedFood.VegetarianPreference = 100;
            }
            if (hasHalal) combinedFood.HalalPreference = 100;
            if (hasKosher) combinedFood.KosherPreference = 100;
            if (hasGlutenFree) combinedFood.GlutenFreePreference = 100;
            if (hasDairyFree) combinedFood.DairyFreePreference = 100;
            if (hasNutAllergy) combinedFood.NutFreePreference = 100;

            var requiredFoodCount = 4 * dayCount;
            var selectedFoodPrefs = new HashSet<string>();
            
            for (int i = 0; i < requiredFoodCount; i++)
            {
                var availablePrefs = foodPreferences
                    .Where(fp => !selectedFoodPrefs.Contains(fp.Key))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (!availablePrefs.Any())
                {
                    selectedFoodPrefs.Clear(); 
                    availablePrefs = foodPreferences;
                }

                var totalWeight = availablePrefs.Values.Sum();
                var randomNumber = random.NextDouble() * totalWeight;
                var currentWeight = 0.0;

                foreach (var pref in availablePrefs)
                {
                    currentWeight += pref.Value;
                    if (randomNumber <= currentWeight)
                    {
                        selectedFoodPrefs.Add(pref.Key);
                        var prop = typeof(UserFoodPreferences).GetProperty(pref.Key);
                        prop?.SetValue(combinedFood, 100);
                        break;
                    }
                }
            }

            var personalPreferences = new Dictionary<string, double>();
            foreach (var prop in typeof(UserPersonalization).GetProperties())
            {
                if (prop.PropertyType == typeof(int?) && prop.Name.EndsWith("Preference"))
                {
                    var values = preferences
                        .Select(p => (int?)prop.GetValue(p.Personal))
                        .Where(v => v.HasValue)
                        .Select(v => v.Value);

                    if (values.Any())
                    {
                        personalPreferences[prop.Name] = values.Average();
                    }
                }
            }

            var requiredPersonalCount = 3 * dayCount;
            var selectedPersonalPrefs = new HashSet<string>();

            for (int i = 0; i < requiredPersonalCount; i++)
            {
                var availablePrefs = personalPreferences
                    .Where(pp => !selectedPersonalPrefs.Contains(pp.Key))
                    .ToDictionary(x => x.Key, x => x.Value);

                if (!availablePrefs.Any())
                {
                    selectedPersonalPrefs.Clear(); 
                    availablePrefs = personalPreferences;
                }

                var totalWeight = availablePrefs.Values.Sum();
                var randomNumber = random.NextDouble() * totalWeight;
                var currentWeight = 0.0;

                foreach (var pref in availablePrefs)
                {
                    currentWeight += pref.Value;
                    if (randomNumber <= currentWeight)
                    {
                        selectedPersonalPrefs.Add(pref.Key);
                        var prop = typeof(UserPersonalization).GetProperty(pref.Key);
                        prop?.SetValue(combinedPersonal, 100);
                        break;
                    }
                }
            }

            return (combinedAccommodation, combinedFood, combinedPersonal);
        }

        /// <summary>
        /// Combines user preferences for accommodation, food, and personalization.
        /// This method averages preferences and applies specific rules for dietary restrictions.
        /// </summary>
        private (UserAccommodationPreferences, UserFoodPreferences, UserPersonalization) CombineUserPreferences(
            List<(UserAccommodationPreferences Accommodation, UserFoodPreferences Food, UserPersonalization Personal)> preferences)
        {
            var random = new Random();
            var combinedAccommodation = new UserAccommodationPreferences();
            var combinedFood = new UserFoodPreferences();
            var combinedPersonal = new UserPersonalization();

            int userCount = preferences.Count;

            bool hasVegan = preferences.Any(p => p.Food.VeganPreference > 50);
            bool hasVegetarian = preferences.Any(p => p.Food.VegetarianPreference > 50);
            bool hasHalal = preferences.Any(p => p.Food.HalalPreference > 50);
            bool hasKosher = preferences.Any(p => p.Food.KosherPreference > 50);
            bool hasGlutenFree = preferences.Any(p => p.Food.GlutenFreePreference > 50);
            bool hasDairyFree = preferences.Any(p => p.Food.DairyFreePreference > 50);
            bool hasNutAllergy = preferences.Any(p => p.Food.NutFreePreference > 50);

            foreach (var prop in typeof(UserAccommodationPreferences).GetProperties())
            {
                if (prop.PropertyType == typeof(int?))
                {
                    var values = preferences
                        .Select(p => (int?)prop.GetValue(p.Accommodation))
                        .Where(v => v.HasValue)
                        .ToList();

                    if (values.Any())
                    {
                        var baseAverage = values.Average(v => v.Value);
                        var randomFactor = random.Next(-10, 11);
                        var finalValue = Math.Clamp(baseAverage + randomFactor, 0, 100);
                        prop.SetValue(combinedAccommodation, (int?)finalValue);
                    }
                }
            }

            foreach (var prop in typeof(UserFoodPreferences).GetProperties())
            {
                if (prop.PropertyType == typeof(int?))
                {
                    if (hasVegan && prop.Name == "VeganPreference")
                        prop.SetValue(combinedFood, 100);
                    else if ((hasVegan || hasVegetarian) && prop.Name == "SeafoodPreference")
                        prop.SetValue(combinedFood, 0);
                    else if (hasHalal && prop.Name == "HalalPreference")
                        prop.SetValue(combinedFood, 100);
                    else if (hasKosher && prop.Name == "KosherPreference")
                        prop.SetValue(combinedFood, 100);
                    else if (hasGlutenFree && prop.Name == "GlutenFreePreference")
                        prop.SetValue(combinedFood, 100);
                    else if (hasDairyFree && prop.Name == "DairyFreePreference")
                        prop.SetValue(combinedFood, 100);
                    else if (hasNutAllergy && prop.Name == "NutFreePreference")
                        prop.SetValue(combinedFood, 100);
                    else
                    {
                        var values = preferences
                            .Select(p => (int?)prop.GetValue(p.Food))
                            .Where(v => v.HasValue)
                            .ToList();

                        if (values.Any())
                        {
                            var baseAverage = values.Average(v => v.Value);
                            var randomFactor = random.Next(-15, 16);
                            var finalValue = Math.Clamp(baseAverage + randomFactor, 0, 100);
                            prop.SetValue(combinedFood, (int?)finalValue);
                        }
                    }
                }
            }

            foreach (var prop in typeof(UserPersonalization).GetProperties())
            {
                if (prop.PropertyType == typeof(int?))
                {
                    var values = preferences
                        .Select(p => (int?)prop.GetValue(p.Personal))
                        .Where(v => v.HasValue)
                        .ToList();

                    if (values.Any())
                    {
                        var baseAverage = values.Average(v => v.Value);
                        var randomFactor = random.Next(-20, 21);
                        var finalValue = Math.Clamp(baseAverage + randomFactor, 0, 100);
                        prop.SetValue(combinedPersonal, (int?)finalValue);
                    }
                }
            }

            return (combinedAccommodation, combinedFood, combinedPersonal);
        }

        /// <summary>
        /// Retrieves detailed place information from Google Places API.
        /// This method searches for a place by name and destination, then fetches detailed information.
        /// </summary>
        private async Task<JObject> GetPlaceDetails(string placeName, string destination)
        {
            var searchResponse = await _googlePlacesService.SearchPlacesAsync($"{placeName} {destination}");
            if (searchResponse?.Places?.FirstOrDefault()?.Id != null)
            {
                var details = await _googlePlacesService.GetPlaceDetailsAsync(searchResponse.Places.FirstOrDefault().Id);
                return JObject.Parse(details);
            }
            return null;
        }

        private async Task<TravelAccomodation> GetAccomodationDetails(string placeName, string destination) 
        {
            var searchResponse = await _googlePlacesService.SearchPlacesAsync($"{placeName} {destination}");
            if (searchResponse?.Places?.FirstOrDefault()?.Id != null)
            {
                var details = await _googlePlacesService.GetPlaceDetailsAsync(searchResponse.Places.FirstOrDefault().Id);
                TravelAccomodation x = JsonConvert.DeserializeObject<TravelAccomodation>(details);
                x.GoogleId = searchResponse.Places.FirstOrDefault().Id;
                return x;
            }
            return null;
        }

        /// <summary>
        /// Creates a travel route for users without requiring login. This method:
        /// 1. Generates a complete travel itinerary using AI recommendations
        /// 2. Retrieves detailed place information from Google Places API
        /// 3. Creates a structured route with accommodations, meals, and activities
        /// 4. Saves the route under an admin account for anonymous access
        /// </summary>
        /// <param name="destination">The destination city or location for the route</param>
        /// <param name="startDate">The start date of the travel period (must be within 1 year from current date)</param>
        /// <param name="endDate">The end date of the travel period (maximum 5 days from start date)</param>
        /// <param name="_PRICE_LEVEL">The preferred price level for accommodations and activities (defaults to MODERATE if not specified)</param>
        /// <returns>A response containing the complete route details including places, descriptions, and metadata</returns>
        /// <exception cref="ArgumentNullException">Thrown when destination, startDate, or endDate is null</exception>
        public async Task<CreateRouteWithoutLoginCommandResponseBody> CreateRouteWithoutLogin(string destination, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL)
        {
            destination = char.ToUpper(destination[0]) + destination[1..].ToLowerInvariant();

            if (destination == null || startDate == null || endDate == null)
                throw new ArgumentNullException();

            var dayCount = (endDate.Value.DayNumber - startDate.Value.DayNumber);
            if (dayCount == 0) dayCount = 1;

            var days = await _geminiAIService.CreateRouteAnonymous(destination, dayCount, startDate, endDate, _PRICE_LEVEL);

            var admin = await _userManager.FindByNameAsync("admin");

            StandardRoute standardRoute = new StandardRoute();
            standardRoute.City = destination;
            standardRoute.User = admin;
            standardRoute.LikedUsers = new List<AppUser>();
            standardRoute.name = destination;
            standardRoute.UserId = admin.Id;
            standardRoute.Days = dayCount;
            standardRoute.LikeCount = 0;
            standardRoute.TravelDays = new List<TravelDay>(dayCount);
            standardRoute.status = 2;

            List<string> routeComponentsIds = new List<string>();

            for (int i = 0; i < days.Count; i++)
            {
                routeComponentsIds.Add(days[i].AccomodationPlaceName);
                routeComponentsIds.Add(days[i].BreakfastPlaceName);
                routeComponentsIds.Add(days[i].LunchPlaceName);
                routeComponentsIds.Add(days[i].DinnerPlaceName);
                routeComponentsIds.Add(days[i].FirstPlaceName);
                routeComponentsIds.Add(days[i].SecondPlaceName);
                routeComponentsIds.Add(days[i].ThirdPlaceName);
                routeComponentsIds.Add(days[i].AfterDinnerPlaceName);
            }

            List<PlaceSearchResponse> placeIds = new List<PlaceSearchResponse>();
            List<string> placeDetails = new List<string>();

            for (int i = 0; i < routeComponentsIds.Count; i++)
            {
                var searchResponse = await _googlePlacesService.SearchPlacesAsync(routeComponentsIds[i]);
                if (searchResponse?.Places?.FirstOrDefault()?.Id != null)
                {
                    placeIds.Add(searchResponse);
                    var details = await _googlePlacesService.GetPlaceDetailsAsync(searchResponse.Places.FirstOrDefault().Id);
                    placeDetails.Add(details);
                }
            }

            for (int i = 0; i < days.Count; i++)
            {
                TravelDay travelDay = new TravelDay
                {
                    DayDescription = days[i].DayDescription,
                    User = admin,
                    approxPrice = days[i].ApproxPrice != null ? double.Parse(days[i].ApproxPrice) : 1000
                };

                var jsonSettings = new JsonSerializerSettings
                {
                    NullValueHandling = NullValueHandling.Include,
                    Error = (sender, args) =>
                    {
                        args.ErrorContext.Handled = true;
                    },
                    ContractResolver = new CamelCasePropertyNamesContractResolver(),
                    Converters = new List<JsonConverter>
                    {
                        new StringEnumConverter()
                    }
                };

                var accomodationIndex = i * 8;
                if (placeIds.Count > accomodationIndex && placeIds[accomodationIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[accomodationIndex]);

                    var accomodationDetails = new EEaseWebAPI.Domain.Entities.Route.TravelAccomodation
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        Star = placeDetail["star"]?.Value<string>(),
                        GoogleId = placeIds[accomodationIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        GoodForChildren = placeDetail["goodForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        InternationalPhoneNumber = placeDetail["internationalPhoneNumber"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = placeDetail["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]?["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]?["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]?["acceptsCashOnly"]?.ToString()
                        } : null
                    };



                    travelDay.Accomodation = accomodationDetails;
                }

                var breakfastIndex = accomodationIndex + 1;
                var lunchIndex = accomodationIndex + 2;
                var dinnerIndex = accomodationIndex + 3;

                if (placeIds.Count > breakfastIndex && placeIds[breakfastIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[breakfastIndex]);
                    var breakfastDetails = new EEaseWebAPI.Domain.Entities.Route.Breakfast
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = placeIds[breakfastIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = placeDetail["shortFormattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        Reservable = placeDetail["reservable"]?.Value<bool>(),
                        ServesBrunch = placeDetail["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = placeDetail["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = placeDetail["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = placeDetail["liveMusic"]?.Value<bool>(),
                        MenuForChildren = placeDetail["menuForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        GoodForGroups = placeDetail["goodForGroups"]?.Value<bool>(),
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = placeDetail["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = startDate.HasValue ? await _geminiAIService.GetWeatherForDateAsync(destination, startDate.Value.AddDays(i), new TimeOnly(9, 0)) : null
                    };



                    travelDay.Breakfast = breakfastDetails;
                }

                if (placeIds.Count > lunchIndex && placeIds[lunchIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[lunchIndex]);
                    var lunchDetails = new EEaseWebAPI.Domain.Entities.Route.Lunch
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = placeIds[lunchIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = placeDetail["shortFormattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        Reservable = placeDetail["reservable"]?.Value<bool>(),
                        ServesBrunch = placeDetail["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = placeDetail["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = placeDetail["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = placeDetail["liveMusic"]?.Value<bool>(),
                        MenuForChildren = placeDetail["menuForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        GoodForGroups = placeDetail["goodForGroups"]?.Value<bool>(),
                        ServesBeer = placeDetail["servesBeer"]?.Value<bool>(),
                        ServesWine = placeDetail["servesWine"]?.Value<bool>(),
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = placeDetail["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = startDate.HasValue ? await _geminiAIService.GetWeatherForDateAsync(destination, startDate.Value.AddDays(i), new TimeOnly(12, 0)) : null
                    };



                    travelDay.Lunch = lunchDetails;
                }

                if (placeIds.Count > dinnerIndex && placeIds[dinnerIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[dinnerIndex]);
                    var dinnerDetails = new EEaseWebAPI.Domain.Entities.Route.Dinner
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = placeIds[dinnerIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = placeDetail["shortFormattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        Reservable = placeDetail["reservable"]?.Value<bool>(),
                        ServesBrunch = placeDetail["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = placeDetail["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = placeDetail["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = placeDetail["liveMusic"]?.Value<bool>(),
                        MenuForChildren = placeDetail["menuForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        GoodForGroups = placeDetail["goodForGroups"]?.Value<bool>(),
                        ServesBeer = placeDetail["servesBeer"]?.Value<bool>(),
                        ServesWine = placeDetail["servesWine"]?.Value<bool>(),
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = placeDetail["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = startDate.HasValue ? await _geminiAIService.GetWeatherForDateAsync(destination, startDate.Value.AddDays(i), new TimeOnly(19, 0)) : null
                    };

                    travelDay.Dinner = dinnerDetails;
                }

                var firstPlaceIndex = accomodationIndex + 4;
                var secondPlaceIndex = accomodationIndex + 5;
                var thirdPlaceIndex = accomodationIndex + 6;
                var afterDinnerIndex = accomodationIndex + 7;

                if (placeIds.Count > firstPlaceIndex && placeIds[firstPlaceIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[firstPlaceIndex]);
                    var firstPlaceDetails = new EEaseWebAPI.Domain.Entities.Route.Place
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = placeIds[firstPlaceIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        GoodForChildren = placeDetail["goodForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        Location = placeDetail["location"] != null ? new Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = travelDay.Breakfast.Weather
                    };



                    travelDay.FirstPlace = firstPlaceDetails;
                }

                if (placeIds.Count > secondPlaceIndex && placeIds[secondPlaceIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[secondPlaceIndex]);
                    var secondPlaceDetails = new EEaseWebAPI.Domain.Entities.Route.Place
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = placeIds[secondPlaceIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        GoodForChildren = placeDetail["goodForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        Location = placeDetail["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = travelDay.Lunch.Weather
                    };

                    travelDay.SecondPlace = secondPlaceDetails;
                }

                if (placeIds.Count > thirdPlaceIndex && placeIds[thirdPlaceIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[thirdPlaceIndex]);
                    var thirdPlaceDetails = new EEaseWebAPI.Domain.Entities.Route.Place
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = placeIds[thirdPlaceIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        GoodForChildren = placeDetail["goodForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        Location = placeDetail["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = travelDay.Lunch.Weather
                    };

                    travelDay.ThirdPlace = thirdPlaceDetails;
                }

                if (placeIds.Count > afterDinnerIndex && placeIds[afterDinnerIndex]?.Places?.FirstOrDefault() != null)
                {
                    var placeDetail = JObject.Parse(placeDetails[afterDinnerIndex]);
                    var afterDinnerDetails = new EEaseWebAPI.Domain.Entities.Route.PlaceAfterDinner
                    {
                        _PRICE_LEVEL = _PRICE_LEVEL,
                        GoogleId = placeIds[afterDinnerIndex].Places?.FirstOrDefault()?.Id,
                        FormattedAddress = placeDetail["formattedAddress"]?.ToString(),
                        ShortFormattedAddress = placeDetail["shortFormattedAddress"]?.ToString(),
                        Rating = placeDetail["rating"]?.Value<double>(),
                        GoogleMapsUri = placeDetail["googleMapsUri"]?.ToString(),
                        WebsiteUri = placeDetail["websiteUri"]?.ToString(),
                        NationalPhoneNumber = placeDetail["nationalPhoneNumber"]?.ToString(),
                        PrimaryType = placeDetail["primaryType"]?.ToString(),
                        Reservable = placeDetail["reservable"]?.Value<bool>(),
                        ServesBrunch = placeDetail["servesBrunch"]?.Value<bool>(),
                        ServesVegetarianFood = placeDetail["servesVegetarianFood"]?.Value<bool>(),
                        OutdoorSeating = placeDetail["outdoorSeating"]?.Value<bool>(),
                        LiveMusic = placeDetail["liveMusic"]?.Value<bool>(),
                        MenuForChildren = placeDetail["menuForChildren"]?.Value<bool>(),
                        Restroom = placeDetail["restroom"]?.Value<bool>(),
                        GoodForGroups = placeDetail["goodForGroups"]?.Value<bool>(),
                        GoodForChildren = placeDetail["goodForChildren"]?.Value<bool>(),
                        Takeout = placeDetail["takeout"]?.Value<bool>(),
                        Delivery = placeDetail["delivery"]?.Value<bool>(),
                        CurbsidePickup = placeDetail["curbsidePickup"]?.Value<bool>(),
                        ServesBeer = placeDetail["servesBeer"]?.Value<bool>(),
                        ServesWine = placeDetail["servesWine"]?.Value<bool>(),
                        ServesCocktails = placeDetail["servesCocktails"]?.Value<bool>(),
                        DisplayName = placeDetail["displayName"] != null ? new EEaseWebAPI.Domain.Entities.Route.DisplayName
                        {
                            Text = placeDetail["displayName"]?["text"]?.ToString(),
                            LangugageCode = placeDetail["displayName"]?["languageCode"]?.ToString()
                        } : null,
                        Location = placeDetail["location"] != null ? new EEaseWebAPI.Domain.Entities.Route.Location
                        {
                            Latitude = placeDetail["location"]?["latitude"]?.Value<double>(),
                            Longitude = placeDetail["location"]?["longitude"]?.Value<double>()
                        } : null,
                        RegularOpeningHours = placeDetail["regularOpeningHours"] != null ? new RegularOpeningHours
                        {
                            OpenNow = placeDetail["regularOpeningHours"]?["openNow"]?.Value<bool>(),
                            WeekdayDescriptions = placeDetail["regularOpeningHours"]?["weekdayDescriptions"]?.ToObject<List<string>>(),
                            Periods = placeDetail["regularOpeningHours"]?["periods"]?.ToObject<List<Period>>()
                        } : null,
                        PaymentOptions = placeDetail["paymentOptions"] != null ? new PaymentOptions
                        {
                            AcceptsCreditCards = placeDetail["paymentOptions"]["acceptsCreditCards"]?.ToString(),
                            AcceptsDebitCards = placeDetail["paymentOptions"]["acceptsDebitCards"]?.ToString(),
                            AcceptsCashOnly = placeDetail["paymentOptions"]["acceptsCashOnly"]?.ToString()
                        } : null,
                        Photos = placeDetail["photos"]?.Select(p => new Photos
                        {
                            Name = p["name"]?.ToString(),
                            HeightPx = p["heightPx"]?.Value<int>(),
                            WidthPx = p["widthPx"]?.Value<int>()
                        }).ToList(),
                        Weather = travelDay.Dinner.Weather
                    };

                    travelDay.PlaceAfterDinner = afterDinnerDetails;
                }

                standardRoute.TravelDays.Add(travelDay);
            }

            await _context.StandardRoutes.AddAsync(standardRoute);
            await _context.SaveChangesAsync();

            var routeDto = new CreateRouteWithoutLoginCommandResponseBody
            {
                Route = new StandardRoute
                {
                    Id = standardRoute.Id,
                    City = standardRoute.City,
                    status = standardRoute.status,
                    Days = standardRoute.Days,
                    name = standardRoute.name,
                    UserId = standardRoute.UserId,
                    LikeCount = standardRoute.LikeCount,
                    TravelDays = standardRoute.TravelDays.Select(day => new TravelDay
                    {
                        Id = day.Id,
                        DayDescription = day.DayDescription,
                        approxPrice = day.approxPrice,
                        Accomodation = day.Accomodation,
                        Breakfast = day.Breakfast,
                        Lunch = day.Lunch,
                        Dinner = day.Dinner,
                        FirstPlace = day.FirstPlace,
                        SecondPlace = day.SecondPlace,
                        ThirdPlace = day.ThirdPlace,
                        PlaceAfterDinner = day.PlaceAfterDinner,
                    }).ToList()
                }
            };

            return routeDto;
        }

        /// <summary>
        /// Calculates the number of days between two dates.
        /// </summary>
        /// <param name="startDate">The start date of the period</param>
        /// <param name="endDate">The end date of the period</param>
        /// <returns>The number of days between the start and end dates, or 1 if dates are the same</returns>
        public static int CalculateDaysDifference(DateOnly? startDate, DateOnly? endDate)
        {
            if (startDate == endDate) return 1;
            int difference = (endDate.Value.ToDateTime(TimeOnly.MinValue) - startDate.Value.ToDateTime(TimeOnly.MinValue)).Days;

            return Math.Abs(difference);
        }

        /// <summary>
        /// Retrieves all routes created by a specific user with pagination support.
        /// </summary>
        /// <param name="username">The username of the user whose routes to retrieve</param>
        /// <param name="pageNumber">The page number to retrieve (1-based indexing)</param>
        /// <param name="pageSize">The number of routes per page</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed</param>
        /// <returns>A tuple containing a list of routes for the specified page and the total count of all routes</returns>
        /// <exception cref="UserNotFoundException">Thrown when the specified user is not found</exception>
        public async Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetAllRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.",7);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .AsSplitQuery()  //.AsQueryble()
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Weather)
                .Where(r => r.UserId == user.Id)
                .OrderByDescending(r => r.CreatedDate);

            var totalCount = await query.CountAsync(cancellationToken);

            var routes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(r => new StandardRouteDTO
                {
                    Id = r.Id,
                    City = r.City,
                    Days = r.Days,
                    Name = r.name,
                    CreatedDate = r.CreatedDate,
                    LikeCount = r.LikeCount,
                    UserId = r.UserId,
                    Status = r.status,
                    TravelDays = r.TravelDays.Select(td => new TravelDay
                    {
                        Id = td.Id,
                        DayDescription = td.DayDescription,
                        approxPrice = td.approxPrice,
                        Accomodation = td.Accomodation,
                        Breakfast = td.Breakfast,
                        Lunch = td.Lunch,
                        Dinner = td.Dinner,
                        FirstPlace = td.FirstPlace,
                        SecondPlace = td.SecondPlace,
                        ThirdPlace = td.ThirdPlace,
                        PlaceAfterDinner = td.PlaceAfterDinner
                    }).ToList()
                })
                .ToListAsync(cancellationToken);

            return (routes, totalCount);
        }

        /// <summary>
        /// Retrieves all routes liked by a specific user with pagination support.
        /// For routes that are no longer accessible due to privacy settings:
        /// - Basic information (Id, City, Name, CreatedDate) will still be visible
        /// - Detailed information (TravelDays) will be hidden
        /// - A message explaining why the route is not accessible will be provided
        /// Access rules:
        /// - Private routes: Only visible to owner
        /// - Friends-only routes: Visible to owner and friends
        /// - Public routes: Visible to everyone
        /// </summary>
        /// <param name="username">The username of the user whose liked routes to retrieve</param>
        /// <param name="pageNumber">The page number to retrieve (1-based indexing)</param>
        /// <param name="pageSize">The number of routes per page</param>
        /// <param name="cancellationToken">A token to cancel the operation if needed</param>
        /// <returns>A tuple containing a list of liked routes (with accessibility information) and the total count for pagination</returns>
        /// <exception cref="UserNotFoundException">Thrown when the specified user is not found</exception>
        public async Task<(List<StandardRouteDTO> Routes, int TotalCount)> GetLikedRoutes(string username, int pageNumber = 1, int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", 7);

            var query = _context.StandardRoutes
                .AsNoTracking()
                .AsSplitQuery()
                .Include(r => r.LikedUsers)
                .Include(r => r.User)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Weather)
                .Where(r => r.LikedUsers.Any(u => u.Id == user.Id))
                .OrderByDescending(r => r.CreatedDate);

            var totalCount = await query.CountAsync(cancellationToken);

            var likedRoutes = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(cancellationToken);

            var result = new List<StandardRouteDTO>();

            foreach (var route in likedRoutes)
            {
                var dto = new StandardRouteDTO
                {
                    Id = route.Id,
                    City = route.City,
                    Days = route.Days,
                    Name = route.name,
                    CreatedDate = route.CreatedDate,
                    LikeCount = route.LikeCount,
                    UserId = route.UserId,
                    Status = route.status,
                    IsAccessible = true
                };

                if (route.UserId != user.Id) 
                {
                    switch (route.status)
                    {
                        case 0: 
                            dto.IsAccessible = false;
                            dto.AccessibilityMessage = "This route is now private and only accessible to its owner";
                            dto.TravelDays = null;
                            dto.Status = null;
                            break;

                        case 1: 
                            var isFriend = await _userService.IsFriendAsync(route.User.UserName, username);
                            if (!isFriend)
                            {
                                dto.IsAccessible = false;
                                dto.AccessibilityMessage = "This route is now only accessible to friends";
                                dto.TravelDays = null;
                                dto.Status = null;
                            }
                            else
                            {
                                dto.TravelDays = route.TravelDays.Select(td => new TravelDay
                                {
                                    Id = td.Id,
                                    DayDescription = td.DayDescription,
                                    approxPrice = td.approxPrice,
                                    Accomodation = td.Accomodation,
                                    Breakfast = td.Breakfast,
                                    Lunch = td.Lunch,
                                    Dinner = td.Dinner,
                                    FirstPlace = td.FirstPlace,
                                    SecondPlace = td.SecondPlace,
                                    ThirdPlace = td.ThirdPlace,
                                    PlaceAfterDinner = td.PlaceAfterDinner
                                }).ToList();
                            }
                            break;

                        case 2: 
                            dto.TravelDays = route.TravelDays.Select(td => new TravelDay
                            {
                                Id = td.Id,
                                DayDescription = td.DayDescription,
                                approxPrice = td.approxPrice,
                                Accomodation = td.Accomodation,
                                Breakfast = td.Breakfast,
                                Lunch = td.Lunch,
                                Dinner = td.Dinner,
                                FirstPlace = td.FirstPlace,
                                SecondPlace = td.SecondPlace,
                                ThirdPlace = td.ThirdPlace,
                                PlaceAfterDinner = td.PlaceAfterDinner
                            }).ToList();
                            break;

                        default:
                            dto.IsAccessible = false;
                            dto.AccessibilityMessage = "Invalid route status";
                            dto.TravelDays = null;
                            dto.Status = null;
                            break;
                    }
                }
                else 
                {
                    dto.TravelDays = route.TravelDays.Select(td => new TravelDay
                    {
                        Id = td.Id,
                        DayDescription = td.DayDescription,
                        approxPrice = td.approxPrice,
                        Accomodation = td.Accomodation,
                        Breakfast = td.Breakfast,
                        Lunch = td.Lunch,
                        Dinner = td.Dinner,
                        FirstPlace = td.FirstPlace,
                        SecondPlace = td.SecondPlace,
                        ThirdPlace = td.ThirdPlace,
                        PlaceAfterDinner = td.PlaceAfterDinner
                    }).ToList();
                }

                result.Add(dto);
            }

            return (result, totalCount);
        }

        /// <summary>
        /// Likes or unlikes a route for a specific user.
        /// This method toggles the like status of a route for a user and updates the like count.
        /// Checks route visibility status before allowing like operation:
        /// - Status 0: Private, only owner can like
        /// - Status 1: Friends only, owner and friends can like
        /// - Status 2: Public, anyone can like
        /// </summary>
        public async Task<bool> LikeRoute(string username, Guid routeId)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", 7);

            var route = await _context.StandardRoutes
                .Include(r => r.LikedUsers)
                .Include(r => r.User)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new ArgumentException("Route not found", nameof(routeId));

            switch (route.status)
            {
                case 0: 
                    if (route.UserId != user.Id)
                        throw new UnauthorizedAccessException("This route is private");
                    break;

                case 1:
                    if (route.UserId != user.Id)
                    {
                        var isFriend = await _userService.IsFriendAsync(route.User.UserName, username);
                        if (!isFriend)
                            throw new UnauthorizedAccessException("This route is only visible to friends");
                    }
                    break;

                case 2: 
                    break;

                default:
                    throw new ArgumentException("Invalid route status");
            }

            var isLiked = route.LikedUsers.Any(u => u.Id == user.Id);
            if (isLiked)
            {
                route.LikedUsers.Remove(user);
                route.LikeCount--;
            }
            else
            {
                route.LikedUsers.Add(user);
                route.LikeCount++;
            }

            await _context.SaveChangesAsync();
            return !isLiked; 
        }

        /// <summary>
        /// Deletes a route created by a specific user.
        /// This method removes a route from the database if the user is authorized to delete it.
        /// </summary>
        public async Task<bool> DeleteRoute(string username, Guid? routeId)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User Not Found.", 7);

            var route = await _context.StandardRoutes
                .Include(r => r.TravelDays)
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new RouteNotFoundException("Route not found", 93);

            if (route.UserId != user.Id)
                throw new DeleteRouteException("Unauthorized to delete route");

            _context.StandardRoutes.Remove(route);
            await _context.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Retrieves a specific route by its ID with access control based on route status.
        /// This method checks the route's visibility status and user's permissions:
        /// - Status 0: Private, only visible to route owner
        /// - Status 1: Friends only, visible to route owner and their friends
        /// - Status 2: Public, visible to everyone
        /// </summary>
        /// <param name="username">The username of the user requesting the route</param>
        /// <param name="routeId">The unique identifier of the route to retrieve</param>
        /// <returns>The requested route if the user has permission to view it</returns>
        /// <exception cref="UserNotFoundException">Thrown when the requesting user is not found</exception>
        /// <exception cref="RouteNotFoundException">Thrown when the requested route is not found</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user doesn't have permission to view the route</exception>
        public async Task<StandardRouteDTO> GetRouteById(string username, Guid? routeId)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

            var route = await _context.StandardRoutes
                .Include(r => r.User)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Accomodation)
                        .ThenInclude(a => a.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Breakfast)
                        .ThenInclude(b => b.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Lunch)
                        .ThenInclude(l => l.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.Dinner)
                        .ThenInclude(d => d.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.FirstPlace)
                        .ThenInclude(fp => fp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.SecondPlace)
                        .ThenInclude(sp => sp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.ThirdPlace)
                        .ThenInclude(tp => tp.Weather)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.DisplayName)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Location)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.RegularOpeningHours)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.PaymentOptions)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Photos)
                .Include(r => r.TravelDays)
                    .ThenInclude(td => td.PlaceAfterDinner)
                        .ThenInclude(pad => pad.Weather)
                .AsSplitQuery()
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new RouteNotFoundException("Route not found", 93);



            var dto = new StandardRouteDTO
            {
                Id = route.Id,
                City = route.City,
                Days = route.Days,
                Name = route.name,
                CreatedDate = route.CreatedDate,
                LikeCount = route.LikeCount,
                UserId = route.UserId,
                Status = route.status,
                IsAccessible = true
            };

            if (route.UserId != user.Id) 
            {
                switch (route.status)
                {
                    case 0:
                        dto.IsAccessible = false;
                        dto.AccessibilityMessage = "This route is private and only accessible to its owner";
                        dto.TravelDays = null;
                        dto.Status = null;
                        break;

                    case 1:
                        var isFriend = await _userService.IsFriendAsync(route.User.UserName, username);
                        if (!isFriend)
                            dto.IsAccessible = false;
                            dto.AccessibilityMessage = "This route is only accessible to friends";
                            dto.TravelDays = null;
                            dto.Status = null;
                        break;

                    case 2: 

                        dto.TravelDays = route.TravelDays.Select(td => new TravelDay
                        {
                            Id = td.Id,
                            DayDescription = td.DayDescription,
                            approxPrice = td.approxPrice,
                            Accomodation = td.Accomodation,
                            Breakfast = td.Breakfast,
                            Lunch = td.Lunch,
                            Dinner = td.Dinner,
                            FirstPlace = td.FirstPlace,
                            SecondPlace = td.SecondPlace,
                            ThirdPlace = td.ThirdPlace,
                            PlaceAfterDinner = td.PlaceAfterDinner
                        }).ToList();
                        break;

                    default:
                        dto.IsAccessible = false;
                        dto.AccessibilityMessage = "Invalid route status";
                        dto.TravelDays = null;
                        dto.Status = null;
                        break;
                }
            }
            else 
            {
                dto.TravelDays = route.TravelDays.Select(td => new TravelDay
            {
                Id = td.Id,
                    DayDescription = td.DayDescription,
                    approxPrice = td.approxPrice,
                    Accomodation = td.Accomodation,
                    Breakfast = td.Breakfast,
                    Lunch = td.Lunch,
                    Dinner = td.Dinner,
                    FirstPlace = td.FirstPlace,
                    SecondPlace = td.SecondPlace,
                    ThirdPlace = td.ThirdPlace,
                    PlaceAfterDinner = td.PlaceAfterDinner
            }).ToList();
            }

            return dto;
        }

        /// <summary>
        /// Updates the visibility status of a route.
        /// Status values:
        /// - 0: Private (Only visible to route owner)
        /// - 1: Friends Only (Visible to friends only)
        /// - 2: Public (Visible to everyone)
        /// </summary>
        /// <param name="routeId">The unique identifier of the route to update</param>
        /// <param name="status">New visibility status (0: Private, 1: Friends Only, 2: Public)</param>
        /// <param name="username">Username of the user performing the operation</param>
        /// <returns>Response containing whether the update was successful</returns>
        /// <exception cref="UserNotFoundException">Thrown when the user is not found</exception>
        /// <exception cref="RouteNotFoundException">Thrown when the route is not found</exception>
        /// <exception cref="UnauthorizedAccessException">Thrown when the user is not the route owner</exception>
        /// <exception cref="InvalidRouteStatusException">Thrown when an invalid status value is provided</exception>
        public async Task<UpdateRouteStatusCommandResponseBody> UpdateRouteStatusAsync(Guid routeId, int status, string username)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

            var route = await _context.StandardRoutes
                .FirstOrDefaultAsync(r => r.Id == routeId);

            if (route == null)
                throw new RouteNotFoundException("Route not found", 93);

            if (route.UserId != user.Id)
                throw new UnauthorizedAccessException("Only route owner can update route status");

            if (status < 0 || status > 2)
                throw new InvalidRouteStatusException();

            route.status = status;
            await _context.SaveChangesAsync();

            return new UpdateRouteStatusCommandResponseBody
            {
                IsUpdated = true
            };
        }

        /// <summary>
        /// Retrieves place information based on Google Place ID.
        /// </summary>
        /// <param name="googlePlaceId">Unique place identifier from Google Places API</param>
        /// <param name="placeType">Type of place (accommodation, breakfast, lunch, dinner, placeafterdinner, firstplace, secondplace, thirdplace, place)</param>
        /// <returns>Returns the entity of the found place</returns>
        /// <exception cref="InvalidPlaceTypeException">Thrown when an invalid place type is specified</exception>
        private async Task<BaseEntity> GetPlaceByGoogleId(string googlePlaceId, string placeType)
        {
            switch (placeType.ToLower())
            {
                case "accommodation":
                    return await _context.TravelAccomodations
                        .Include(p => p.DisplayName)
                        .FirstOrDefaultAsync(p => p.GoogleId == googlePlaceId);

                case "breakfast":
                case "lunch":
                case "dinner":
                case "placeafterdinner":
                    var restaurantQuery = _context.Set<BaseRestaurantPlaceEntity>()
                        .Include(p => p.DisplayName)
                        .Where(p => p.GoogleId == googlePlaceId);

                    return placeType.ToLower() switch
                    {
                        "breakfast" => await restaurantQuery.OfType<Breakfast>().FirstOrDefaultAsync(),
                        "lunch" => await restaurantQuery.OfType<Lunch>().FirstOrDefaultAsync(),
                        "dinner" => await restaurantQuery.OfType<Dinner>().FirstOrDefaultAsync(),
                        "placeafterdinner" => await restaurantQuery.OfType<PlaceAfterDinner>().FirstOrDefaultAsync(),
                        _ => null
                    };

                case "firstplace":
                case "secondplace":
                case "thirdplace":
                case "place":
                    return await _context.Places
                        .Include(p => p.DisplayName)
                        .FirstOrDefaultAsync(p => p.GoogleId == googlePlaceId);

                default:
                    throw new InvalidPlaceTypeException($"Invalid place type: {placeType}");
            }
        }

        /// <summary>
        /// Updates user preferences based on liked places or restaurants.
        /// </summary>
        /// <param name="username">Username of the user performing the action</param>
        /// <param name="googlePlaceId">Google Place ID of the liked place</param>
        /// <param name="placeType">Type of the liked place</param>
        /// <returns>Response containing the result of preference update operation</returns>
        /// <exception cref="UserNotFoundException">Thrown when user is not found</exception>
        /// <exception cref="InvalidPlaceTypeException">Thrown when an invalid place type is specified</exception>
        public async Task<LikePlaceOrRestaurantCommandResponseBody> LikePlaceOrRestaurantAsync(
            string username,
            string googlePlaceId,
            string placeType)
        {
            var user = await _userManager.FindByNameAsync(username);
            if (user == null)
                throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

            var place = await GetPlaceByGoogleId(googlePlaceId, placeType.ToLower());
            if (place == null)
                throw new InvalidPlaceTypeException($"Place not found with Google ID: {googlePlaceId}");

            switch (placeType.ToLower())
            {
                case "accommodation":
                    return await UpdateAccommodationPreferences(user.Id, place);

                case "breakfast":
                case "lunch":
                case "dinner":
                case "placeafterdinner":
                    return await UpdateFoodPreferences(user.Id, place, placeType);

                case "firstplace":
                case "secondplace":
                case "thirdplace":
                case "place":
                    return await UpdatePersonalizationPreferences(user.Id, place);

                default:
                    throw new InvalidPlaceTypeException($"Invalid place type: {placeType}. Valid types are: Accommodation, Breakfast, Lunch, Dinner, PlaceAfterDinner, FirstPlace, SecondPlace, ThirdPlace");
            }
        }

        /// <summary>
        /// Updates user's accommodation preferences.
        /// </summary>
        /// <param name="userId">ID of the user whose preferences will be updated</param>
        /// <param name="accommodation">Entity of the liked accommodation</param>
        /// <returns>Response containing the result of preference update operation</returns>
        /// <exception cref="InvalidPlaceTypeException">Thrown when an invalid accommodation type is specified</exception>
        /// <exception cref="BaseException">Thrown when user preferences are not found</exception>
        private async Task<LikePlaceOrRestaurantCommandResponseBody> UpdateAccommodationPreferences(string userId, BaseEntity accommodation)
        {
            var travelAccommodation = accommodation as TravelAccomodation;
            if (travelAccommodation == null)
                throw new InvalidPlaceTypeException("Invalid accommodation type");

            var userAccommodationPrefs = await _context.UserAccommodationPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userAccommodationPrefs == null)
            {
                throw new BaseException("User personalization not found", (int)StatusEnum.PreferenceDescriptionsRetrievalFailed);
            }

            var availablePreferences = typeof(UserAccommodationPreferences)
                .GetProperties()
                .Where(p => p.Name.EndsWith("Preference"))
                .Select(p => p.Name)
                .ToList();

            var accommodationDescription = $"Name: {travelAccommodation.DisplayName?.Text}, " +
                                         $"Type: {travelAccommodation.PrimaryType}, " +
                                         $"Rating: {travelAccommodation.Rating}, " +
                                         $"Price Level: {travelAccommodation._PRICE_LEVEL}";

            var preferencesToUpdate = await _geminiAIService.AnalyzePlacePreferencesAsync(
                travelAccommodation.DisplayName?.Text ?? "Unknown Accommodation",
                "Accommodation",
                accommodationDescription,
                availablePreferences
            );

            var random = new Random();
            var updatedPreferences = new Dictionary<string, int>();
            foreach (var prefName in preferencesToUpdate)
            {
                var property = typeof(UserAccommodationPreferences).GetProperty(prefName);
                if (property != null)
                {
                    var currentValue = (int?)property.GetValue(userAccommodationPrefs) ?? 0;
                    var increment = random.Next(1, 4); 
                    var newValue = Math.Min(100, currentValue + increment);
                    property.SetValue(userAccommodationPrefs, newValue);
                    updatedPreferences.Add(prefName, increment);
                }
            }

            await _context.SaveChangesAsync();
            return new LikePlaceOrRestaurantCommandResponseBody
            {
                IsPreferenceUpdated = true,
                Message = $"Accommodation preferences updated: {string.Join(", ", updatedPreferences.Select(p => $"{p.Key} (+{p.Value})"))}",
            };
        }

        /// <summary>
        /// Updates user's food preferences.
        /// </summary>
        /// <param name="userId">ID of the user whose preferences will be updated</param>
        /// <param name="place">Entity of the liked restaurant</param>
        /// <param name="placeType">Type of restaurant (breakfast, lunch, dinner, placeafterdinner)</param>
        /// <returns>Response containing the result of the preference update operation</returns>
        /// <exception cref="InvalidPlaceTypeException">Thrown when invalid restaurant type is specified</exception>
        /// <exception cref="BaseException">Thrown when user preferences are not found</exception>
        private async Task<LikePlaceOrRestaurantCommandResponseBody> UpdateFoodPreferences(string userId, BaseEntity place, string placeType)
        {
            var restaurant = place as BaseRestaurantPlaceEntity;
            if (restaurant == null)
                throw new InvalidPlaceTypeException("Invalid restaurant type");

            var userFoodPrefs = await _context.UserFoodPreferences
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userFoodPrefs == null)
            {
                throw new BaseException("User personalization not found", (int)StatusEnum.PreferenceDescriptionsRetrievalFailed);
            }

            var availablePreferences = typeof(UserFoodPreferences)
                .GetProperties()
                .Where(p => p.Name.EndsWith("Preference"))
                .Select(p => p.Name)
                .ToList();

            var placeDescription = $"Name: {restaurant.DisplayName?.Text}, " +
                                  $"Type: {restaurant.PrimaryType}, " +
                                  $"Rating: {restaurant.Rating}, " +
                                  $"Price Level: {restaurant._PRICE_LEVEL}, " +
                                  $"Meal Type: {placeType}";

            var preferencesToUpdate = await _geminiAIService.AnalyzePlacePreferencesAsync(
                restaurant.DisplayName?.Text ?? $"Unknown {placeType}",
                placeType,
                placeDescription,
                availablePreferences
            );

            var random = new Random();
            var updatedPreferences = new Dictionary<string, int>();
            foreach (var prefName in preferencesToUpdate)
            {
                var property = typeof(UserFoodPreferences).GetProperty(prefName);
                if (property != null)
                {
                    var currentValue = (int?)property.GetValue(userFoodPrefs) ?? 0;
                    var increment = random.Next(1, 4); 
                    var newValue = Math.Min(100, currentValue + increment);
                    property.SetValue(userFoodPrefs, newValue);
                    updatedPreferences.Add(prefName, increment);
                }
            }

            await _context.SaveChangesAsync();
            return new LikePlaceOrRestaurantCommandResponseBody
            {
                IsPreferenceUpdated = true,
                Message = $"Food preferences updated for {placeType}: {string.Join(", ", updatedPreferences.Select(p => $"{p.Key} (+{p.Value}"))}"
            };
        }

        /// <summary>
        /// Updates user's personalization preferences.
        /// </summary>
        /// <param name="userId">ID of the user whose preferences will be updated</param>
        /// <param name="place">Entity of the liked place</param>
        /// <returns>Response containing the result of the preference update operation</returns>
        /// <exception cref="InvalidPlaceTypeException">Throws when invalid venue type is specified</exception>
        /// <exception cref="BaseException">Thrown when user preferences are not found</exception>
        private async Task<LikePlaceOrRestaurantCommandResponseBody> UpdatePersonalizationPreferences(string userId, BaseEntity place)
        {
            var travelPlace = place as BaseTravelPlaceEntity;
            if (travelPlace == null)
                throw new InvalidPlaceTypeException("Invalid place type");

            var userPersonalization = await _context.UserPersonalizations
                .FirstOrDefaultAsync(up => up.UserId == userId);

            if (userPersonalization == null)
            {
                throw new BaseException("User personalization not found",(int)StatusEnum.PreferenceDescriptionsRetrievalFailed);
            }

            var availablePreferences = typeof(UserPersonalization)
                .GetProperties()
                .Where(p => p.Name.EndsWith("Preference"))
                .Select(p => p.Name)
                .ToList();

            var placeDescription = $"Name: {travelPlace.DisplayName?.Text}, " +
                                  $"Type: {travelPlace.PrimaryType}, " +
                                  $"Rating: {travelPlace.Rating}";

            var preferencesToUpdate = await _geminiAIService.AnalyzePlacePreferencesAsync(
                travelPlace.DisplayName?.Text ?? "Unknown Place",
                travelPlace.PrimaryType ?? "Unknown Type",
                placeDescription,
                availablePreferences
            );

            var random = new Random();
            var updatedPreferences = new Dictionary<string, int>();
            foreach (var prefName in preferencesToUpdate)
            {
                var property = typeof(UserPersonalization).GetProperty(prefName);
                if (property != null)
                {
                    var currentValue = (int?)property.GetValue(userPersonalization) ?? 0;
                    var increment = random.Next(1, 4); 
                    var newValue = Math.Min(100, currentValue + increment);
                    property.SetValue(userPersonalization, newValue);
                    updatedPreferences.Add(prefName, increment);
                }
            }

            await _context.SaveChangesAsync();
            return new LikePlaceOrRestaurantCommandResponseBody
            {
                IsPreferenceUpdated = true,
                Message = $"Personalization preferences updated: {string.Join(", ", updatedPreferences.Select(p => $"{p.Key} (+{p.Value}"))}"
            };
        }

        /// <summary>
        /// Checks if a user has liked a specific route and has permission to view it.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="routeId">ID of the route to check</param>
        /// <returns>True if the user has liked the route, false otherwise</returns>
        /// <exception cref="BaseException">Thrown when any error occurs during the operation</exception>
        public async Task<bool> CheckRouteLikeStatus(string username, Guid routeId)
        {
            try
            {
                var user = await _userManager.FindByNameAsync(username);
                if (user == null)
                    throw new Application.Exceptions.Login.UserNotFoundException("User not found", 7);

                var route = await _context.StandardRoutes
                    .Include(r => r.User)
                    .Include(r => r.LikedUsers)
                    .FirstOrDefaultAsync(r => r.Id == routeId);

                if (route == null)
                    throw new BaseException("Route not found", (int)StatusEnum.RouteNotFound);

                if (route.status == 0 && route.UserId != user.Id)
                    throw new BaseException("This route is private ", (int)StatusEnum.UnauthorizedToViewRoute);

                if (route.status == 1 && route.UserId != user.Id)
                {
                    var isFriend = await _userService.IsFriendAsync(route.User.UserName, username);
                    if (!isFriend)
                        throw new BaseException("This route is private", (int)StatusEnum.UnauthorizedToViewRoute);
                }

                return route.LikedUsers.Any(u => u.Id == user.Id);
            }
            catch (Exception ex)
            {
                throw new BaseException(ex.Message, (int)StatusEnum.UnknownError);
            }
        }
    }
}
