using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Application.DTOs.Route.NewCustomRoute;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.MapEntities.GeminiAI;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Json;
using System.Text.Json;

namespace EEaseWebAPI.Persistence.Services
{
    public class GeminiAIService : IGeminiAIService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiEndpoint;
        private readonly IGeminiKeyManager _keyManager;

        /// <summary>
        /// Constant string containing the description of user preference entities and their scoring scale.
        /// Used for analyzing user messages and extracting preferences for accommodation, food, and personalization.
        /// </summary>
        private const string ENTITY_DESCRIPTION = @"
You need to analyze the user's message and extract preferences according to these entity structures.
All preferences are scored on a scale of 0-100 

IMPORTANT DIETARY RESTRICTIONS:
When analyzing food preferences, pay special attention to dietary restrictions. If a user mentions being:
- Vegan: Automatically set VeganPreference to 100 and set SeafoodPreference, DairyFreePreference to 0
- Vegetarian: Automatically set VegetarianPreference to 100 and set SeafoodPreference to 0
- Halal: Automatically set HalalPreference to 100
- Kosher: Automatically set KosherPreference to 100
- Gluten-Free: Automatically set GlutenFreePreference to 100
- Dairy-Free: Automatically set DairyFreePreference to 100
- Nut Allergy: Automatically set NutFreePreference to 100 and set AllergiesPreference to 100

1. UserAccommodationPreferences (all fields are int, 0-100 scale):
   - LuxuryHotelPreference: Preference for luxury hotels
   - BudgetHotelPreference: Preference for budget hotels
   - BoutiqueHotelPreference: Preference for boutique hotels
   - HostelPreference: Preference for hostels
   - ApartmentPreference: Preference for apartment stays
   - ResortPreference: Preference for resorts
   - VillaPreference: Preference for villas
   - GuestHousePreference: Preference for guesthouses
   - CampingPreference: Preference for camping
   - GlampingPreference: Preference for glamping
   - BedAndBreakfastPreference: Preference for bed and breakfast
   - AllInclusivePreference: Preference for all-inclusive hotels
   - SpaAndWellnessPreference: Preference for spa and wellness hotels
   - PetFriendlyPreference: Preference for pet-friendly accommodations
   - EcoFriendlyPreference: Preference for eco-friendly accommodations
   - RemoteLocationPreference: Preference for remote locations
   - CityCenterPreference: Preference for city center locations
   - FamilyFriendlyPreference: Preference for family-friendly accommodations
   - AdultsOnlyPreference: Preference for adults-only accommodations
   - HomestayPreference: Preference for homestays
   - WaterfrontPreference: Preference for waterfront locations
   - HistoricalBuildingPreference: Preference for historical buildings
   - AirbnbPreference: Preference for Airbnb accommodations
   - CoLivingSpacePreference: Preference for co-living spaces
   - ExtendedStayPreference: Preference for extended stay accommodations

2. UserFoodPreferences (all fields are int, 0-100 scale):
   - VegetarianPreference: Preference for vegetarian food (MUST be 100 if user is vegetarian)
   - VeganPreference: Preference for vegan food (MUST be 100 if user is vegan)
   - GlutenFreePreference: Preference for gluten-free food (MUST be 100 if user has celiac or gluten sensitivity)
   - HalalPreference: Preference for halal food (MUST be 100 if user requires halal)
   - KosherPreference: Preference for kosher food (MUST be 100 if user requires kosher)
   - SeafoodPreference: Preference for seafood (MUST be 0 for vegetarian/vegan users)
   - LocalCuisinePreference: Preference for local cuisine
   - FastFoodPreference: Preference for fast food
   - FinePreference: Preference for fine dining
   - StreetFoodPreference: Preference for street food
   - OrganicPreference: Preference for organic food
   - BuffetPreference: Preference for buffet dining
   - FoodTruckPreference: Preference for food trucks
   - CafeteriaPreference: Preference for cafeteria dining
   - DeliveryPreference: Preference for food delivery
   - AllergiesPreference: Consideration for food allergies (MUST be 100 if any allergy mentioned)
   - DairyFreePreference: Preference for dairy-free food (MUST be 100 if lactose intolerant)
   - NutFreePreference: Preference for nut-free food (MUST be 100 if nut allergy)
   - SpicyPreference: Preference for spicy food
   - SweetPreference: Preference for sweet food
   - SaltyPreference: Preference for salty food
   - SourPreference: Preference for sour food
   - BitterPreference: Preference for bitter food
   - UmamiPreference: Preference for umami food
   - FusionPreference: Preference for fusion cuisine

3. UserPersonalization (all fields are int, 0-100 scale):
   - AdventurePreference: Preference for adventure activities
   - RelaxationPreference: Preference for relaxation
   - CulturalPreference: Interest in cultural experiences
   - NaturePreference: Interest in nature activities
   - UrbanPreference: Preference for urban environments
   - RuralPreference: Preference for rural environments
   - LuxuryPreference: Preference for luxury experiences
   - BudgetPreference: Preference for budget-friendly options
   - SoloTravelPreference: Preference for solo travel
   - GroupTravelPreference: Preference for group travel
   - FamilyTravelPreference: Preference for family travel
   - CoupleTravelPreference: Preference for couple travel
   - BeachPreference: Interest in beach activities
   - MountainPreference: Interest in mountain activities
   - DesertPreference: Interest in desert environments
   - ForestPreference: Interest in forest environments
   - IslandPreference: Interest in island destinations
   - LakePreference: Interest in lake activities
   - RiverPreference: Interest in river activities
   - WaterfallPreference: Interest in waterfall destinations
   - CavePreference: Interest in cave exploration
   - VolcanoPreference: Interest in volcanic sites
   - GlacierPreference: Interest in glacier experiences
   - CanyonPreference: Interest in canyon exploration
   - ValleyPreference: Interest in valley destinations

CRITICAL RULES FOR DIETARY RESTRICTIONS:
1. Dietary restrictions MUST be treated as absolute rules, not preferences
2. If a user mentions being vegan, they CANNOT have seafood or dairy preferences
3. If a user mentions food allergies, those MUST be marked as 100 and considered in all recommendations
4. Religious dietary requirements (Halal/Kosher) MUST be strictly respected
5. When in doubt about a dietary restriction, ask for clarification rather than making assumptions
6. I Want you to think as a real human what user likes. Do not fill fields only 25 50 75 100, you can use any number between 0 - 100 , example: ValleyPreference = 53.
7. You have to choose at least 5 attributes per of these 3 personalization, food, accomodation";

        /// <summary>
        /// Initializes a new instance of the GeminiAIService with required dependencies.
        /// </summary>
        /// <param name="configuration">Application configuration for accessing API settings</param>
        /// <param name="httpClient">HTTP client for making API requests</param>
        /// <param name="keyManager">Service for managing Gemini API keys</param>
        public GeminiAIService(IConfiguration configuration, HttpClient httpClient, IGeminiKeyManager keyManager)
        {
            _httpClient = httpClient;
            _apiEndpoint = configuration["GeminiAI:ApiEndpoint"];
            _keyManager = keyManager;
            
            _httpClient.BaseAddress = new Uri(_apiEndpoint);
        }

        /// <summary>
        /// Extracts JSON content from a Gemini API response by finding the first valid JSON object in the text.
        /// </summary>
        /// <param name="response">The parsed Gemini API response</param>
        /// <returns>The extracted JSON string, or null if no valid JSON is found</returns>
        private string? ExtractJsonFromGeminiResponse(GeminiResponse? response)
        {
            if (response?.Candidates == null || response.Candidates.Length == 0)
                return null;

            var content = response.Candidates[0]?.Content;
            if (content?.Parts == null || content.Parts.Length == 0)
                return null;

            var text = content.Parts[0]?.Text;
            if (string.IsNullOrEmpty(text))
                return null;

            text = text.Replace("```json", "").Replace("```", "").Trim();

            var jsonStart = text.IndexOf('{');
            var jsonEnd = text.LastIndexOf('}');
            
            if (jsonStart == -1 || jsonEnd == -1)
                return null;

            return text.Substring(jsonStart, jsonEnd - jsonStart + 1);
        }

        /// <summary>
        /// Sends a prompt to the Gemini AI API and returns the generated content.
        /// Handles API key management, request/response processing, and error handling.
        /// </summary>
        /// <param name="prompt">The text prompt to send to the API</param>
        /// <returns>The generated text response from the API</returns>
        /// <exception cref="GeminiInvalidMessageException">Thrown when the prompt is empty or whitespace</exception>
        /// <exception cref="GeminiAPIKeyNotFoundException">Thrown when no API key is available</exception>
        /// <exception cref="GeminiAPIKeyLimitExceededException">Thrown when API quota is exceeded</exception>
        /// <exception cref="GeminiAPIResponseParseException">Thrown when the API response cannot be parsed</exception>
        /// <exception cref="GeminiAIServiceException">Thrown for general API communication errors</exception>
        public async Task<string> GenerateContentAsync(string prompt)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prompt))
                    throw new GeminiInvalidMessageException("The prompt cannot be empty or whitespace.");

                var apiKey = await _keyManager.GetAvailableApiKey();
                if (apiKey == null)
                    throw new GeminiAPIKeyNotFoundException();

                try
                {
                    await _keyManager.MarkKeyAsUsed(apiKey);

                    var requestBody = new
                    {
                        contents = new[]
                        {
                            new
                            {
                                parts = new[]
                                {
                                    new { text = prompt }
                                }
                            }
                        }
                    };

                    var response = await _httpClient.PostAsJsonAsync($"?key={apiKey}", requestBody);
                    response.EnsureSuccessStatusCode();

                    var responseContent = await response.Content.ReadAsStringAsync();
                    var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    if (geminiResponse?.Candidates == null || geminiResponse.Candidates.Length == 0)
                        throw new GeminiAPIResponseParseException("No response candidates found");

                    var content = geminiResponse.Candidates[0]?.Content;
                    if (content?.Parts == null || content.Parts.Length == 0)
                        throw new GeminiAPIResponseParseException("No content parts found in response");

                    var text = content.Parts[0]?.Text;
                    if (string.IsNullOrEmpty(text))
                        throw new GeminiAPIResponseParseException("Empty response text");

                    return text;
                }
                finally
                {
                    await _keyManager.ReleaseKey(apiKey);
                }
            }
            catch (HttpRequestException ex)
            {
                if (ex.Message.Contains("quota"))
                    throw new GeminiAPIKeyLimitExceededException("API key quota exceeded", ex);
                throw new GeminiAIServiceException("Failed to communicate with Gemini AI API", ex);
            }
            catch (JsonException ex)
            {
                throw new GeminiAPIResponseParseException("Failed to parse API response", ex);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new GeminiAIServiceException("Failed to generate content with Gemini AI", ex);
            }
        }

        /// <summary>
        /// Creates a detailed travel itinerary for the specified destination using Gemini AI.
        /// Generates a day-by-day plan including accommodations, meals, activities, and detailed descriptions.
        /// </summary>
        /// <param name="destination">The target destination for the itinerary</param>
        /// <param name="dayCount">Number of days for the trip</param>
        /// <param name="startDate">Start date of the trip for seasonal considerations</param>
        /// <param name="endDate">End date of the trip</param>
        /// <param name="priceLevel">Desired price level for venues and activities</param>
        /// <returns>A list of AnonymousDay objects containing the complete itinerary</returns>
        /// <exception cref="ArgumentNullException">Thrown when destination is null or empty</exception>
        /// <exception cref="GeminiAPIResponseParseException">Thrown when the API response cannot be parsed into valid itinerary data</exception>
        public async Task<List<AnonymousDay>> CreateRouteAnonymous(string? destination, int dayCount, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? priceLevel = null)
        {
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentNullException(nameof(destination));

            var budgetContext = priceLevel switch
            {
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => @"Budget-Friendly",

                PRICE_LEVEL.PRICE_LEVEL_MODERATE => @"Moderate Budget",

                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => @"Luxury Focus",

                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => @"Ultra-Luxury",

                _ => @"Budget-Friendly"
            };

            var seasonalContext = GetSeasonalContext(startDate);
            var prompt = $@"Create a detailed {dayCount}-day travel itinerary for {destination}. Format the response as a JSON object with the following structure:

{{
  ""AnonymousDayList"": [
        {{
      ""DayDescription"": ""Detailed description of the day's activities and flow (cant be null)"",
      ""AccomodationPlaceName"": ""{budgetContext} Specific hotel or accommodation name(think 5 hotel name first and then suggest one of them randomly for variety and every route day has SAME hotel name) (cant be null)"",
      ""BreakfastPlaceName"": ""{budgetContext} Specific restaurant or cafe name, think 5 restaurant name first and then suggest one of them randomly for variety, be aware dont recommend the same place again(cant be null)"",
      ""LunchPlaceName"": ""{budgetContext} Specific restaurant or cafe name, think 5 restaurant name first and then suggest one of them randomly for variety, be aware dont recommend the same place again (cant be null)"",
      ""DinnerPlaceName"": ""{budgetContext} Specific restaurant or cafe name, think 5 restaurant name first and then suggest one of them randomly for variety, be aware dont recommend the same place again (cant be null)"",
      ""FirstPlaceName"": ""{budgetContext} First attraction or activity venue name,think 5 attaction or activity venue name first and then suggest one of them randomly for variety, be aware dont recommend the same place again (cant be null)"",
      ""SecondPlaceName"": ""{budgetContext} Second attraction or activity venue name,think 5 attaction or activity venue name first and then suggest one of them randomly for variety, be aware dont recommend the same place again (cant be null)"",
      ""ThirdPlaceName"": ""{budgetContext} Third attraction or activity venue name,think 5 attaction or activity venue name first and then suggest one of them randomly for variety, be aware dont recommend the same place again (cant be null)"",
      ""AfterDinnerPlaceName"": ""{budgetContext} Evening venue or activity location name,think 5 Evening venue or activity name first and then suggest one of them randomly for variety, be aware dont recommend the same place again (cant be null)"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD(cant be null)""
        }}
    ]
}}
 
### Season: {seasonalContext} ";

            var jsonResponse = await GenerateContentAsync(prompt);
            
            try
            {
                jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                var response = JsonSerializer.Deserialize<GeminiResponseForAnonymousRoute>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response?.AnonymousDayList == null || !response.AnonymousDayList.Any())
                    return await CreateRouteAnonymous(destination, dayCount, startDate, endDate, priceLevel);

                foreach (var day in response.AnonymousDayList)
                {
                    if (string.IsNullOrWhiteSpace(day.AccomodationPlaceName) ||
                        string.IsNullOrWhiteSpace(day.BreakfastPlaceName) ||
                        string.IsNullOrWhiteSpace(day.LunchPlaceName) ||
                        string.IsNullOrWhiteSpace(day.DinnerPlaceName) ||
                        string.IsNullOrWhiteSpace(day.FirstPlaceName) ||
                        string.IsNullOrWhiteSpace(day.SecondPlaceName) ||
                        string.IsNullOrWhiteSpace(day.ThirdPlaceName) ||
                        string.IsNullOrWhiteSpace(day.AfterDinnerPlaceName) ||
                        string.IsNullOrWhiteSpace(day.DayDescription) ||
                        day.AccomodationPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.AccomodationPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        day.BreakfastPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.BreakfastPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        day.LunchPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.LunchPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        day.DinnerPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.DinnerPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        day.FirstPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.FirstPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        day.SecondPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.SecondPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        day.ThirdPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.ThirdPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase) ||
                        day.AfterDinnerPlaceName.Equals("NULL", StringComparison.OrdinalIgnoreCase) ||
                        day.AfterDinnerPlaceName.Equals("N/A", StringComparison.OrdinalIgnoreCase))
                    {
                        return await CreateRouteAnonymous(destination, dayCount, startDate, endDate, priceLevel);
                    }
                }
                

                return response.AnonymousDayList;
            }
            catch (JsonException)
            {
                //try again
                return await CreateRouteAnonymous(destination, dayCount, startDate, endDate, priceLevel);
            }
        }

        /// <summary>
        /// Determines the seasonal context for activities based on the provided date.
        /// Provides recommendations for activities and considerations specific to each season.
        /// </summary>
        /// <param name="date">The date to determine the season for</param>
        /// <returns>A string containing seasonal recommendations and considerations</returns>
        private string GetSeasonalContext(DateOnly? date)
        {
            if (!date.HasValue) return "Consider year-round activities and attractions";

            var month = date.Value.Month;

            return month switch
            {
                12 or 1 or 2 => @"Winter: Indoor attractions, winter sports, cozy dining, seasonal events",
                3 or 4 or 5 => @"Spring: Parks, gardens, festivals, al fresco dining, nature activities",
                6 or 7 or 8 => @"Summer: Outdoor fun, water activities, rooftop dining, summer events",
                _ => @"Autumn: Festivals, foliage walks, seasonal food, flexible activities"
            };

        }

        /// <summary>
        /// Analyzes a user's message to extract their preferences for accommodation, food, and personalization.
        /// Uses Gemini AI to interpret the message and score preferences on a 0-100 scale.
        /// </summary>
        /// <param name="message">The user's message describing their preferences</param>
        /// <returns>A tuple containing the extracted preferences for accommodation, food, and personalization</returns>
        /// <exception cref="GeminiInvalidMessageException">Thrown when the message is empty or whitespace</exception>
        /// <exception cref="GeminiAPIKeyNotFoundException">Thrown when no API key is available</exception>
        /// <exception cref="GeminiAPIKeyLimitExceededException">Thrown when API quota is exceeded</exception>
        /// <exception cref="GeminiAPIResponseParseException">Thrown when preferences cannot be parsed from the response</exception>
        /// <exception cref="GeminiAIServiceException">Thrown for general API communication errors</exception>
        public async Task<(UserAccommodationPreferences, UserFoodPreferences, UserPersonalization)> GetUserPreferencesFromMessage(string message)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(message))
                {
                    throw new GeminiInvalidMessageException("The message cannot be empty or whitespace.");
                }

                var apiKey = await _keyManager.GetAvailableApiKey();
                if (apiKey == null)
                {
                    throw new GeminiAPIKeyNotFoundException();
                }

                try
                {
                    await _keyManager.MarkKeyAsUsed(apiKey);

                    try
                    {
                        var requestBody = new
                        {
                            contents = new[]
                            {
                                new
                                {
                                    parts = new[]
                                    {
                                        new
                                        {
                                            text = $"{ENTITY_DESCRIPTION}\n\nUser Message: {message}\n\nAnalyze this message and return a JSON object with three sections: accommodationPreferences, foodPreferences, and personalization. For each section, include ONLY the relevant fields with scores from 0-100 based on the user's preferences. Exclude any fields that cannot be confidently scored based on the message."
                                        }
                                    }
                                }
                            }
                        };

                        var response = await _httpClient.PostAsJsonAsync($"?key={apiKey}", requestBody);
                        response.EnsureSuccessStatusCode();

                        var responseContent = await response.Content.ReadAsStringAsync();
                        
                        try
                        {
                            var options = new JsonSerializerOptions
                            {
                                PropertyNameCaseInsensitive = true
                            };

                            var geminiResponse = JsonSerializer.Deserialize<GeminiResponse>(responseContent, options);
                            if (geminiResponse == null)
                            {
                                throw new GeminiAPIResponseParseException("Failed to deserialize Gemini AI response.");
                            }

                            if (geminiResponse.Candidates == null || geminiResponse.Candidates.Length == 0)
                            {
                                throw new GeminiAPIResponseParseException("Gemini API response contains no candidates.");
                            }

                            var preferencesJson = ExtractJsonFromGeminiResponse(geminiResponse);
                            if (preferencesJson == null)
                            {
                                throw new GeminiAPIResponseParseException("Failed to extract preferences from Gemini AI response");
                            }

                            var preferences = JsonSerializer.Deserialize<PreferencesResponse>(preferencesJson, options);
                            if (preferences == null)
                            {
                                throw new GeminiAPIResponseParseException("Failed to deserialize preferences JSON");
                            }
                            
                            return (
                                preferences.AccommodationPreferences ?? new UserAccommodationPreferences(),
                                preferences.FoodPreferences ?? new UserFoodPreferences(),
                                preferences.Personalization ?? new UserPersonalization()
                            );
                        }
                        catch (JsonException ex)
                        {
                            throw new GeminiAPIResponseParseException($"Failed to parse JSON response. Raw response: {responseContent}", ex);
                        }
                    }
                    catch (HttpRequestException ex)
                    {
                        if (ex.Message.Contains("quota"))
                        {
                            throw new GeminiAPIKeyLimitExceededException("API key quota exceeded", ex);
                        }
                        throw new GeminiAIServiceException("Failed to communicate with Gemini AI API", ex);
                    }
                    catch (JsonException ex)
                    {
                        throw new GeminiAPIResponseParseException("Failed to parse JSON response", ex);
                    }
                    catch (Exception ex) when (ex is not BaseException)
                    {
                        throw new GeminiAIServiceException("An unexpected error occurred while processing the message", ex);
                    }
                }
                finally
                {
                    await _keyManager.ReleaseKey(apiKey);
                }
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new GeminiAIServiceException("Failed to process preferences with Gemini AI", ex);
            }
        }

        /// <summary>
        /// Retrieves weather information for a specific city, date, and time using Gemini AI.
        /// Provides temperature, weather description, and helpful warnings or advice based on weather conditions.
        /// </summary>
        /// <param name="city">The target city for weather information</param>
        /// <param name="date">The specific date for the weather forecast</param>
        /// <param name="time">The specific time of day for the weather forecast</param>
        /// <returns>A Weather object containing temperature, description, and relevant weather warnings or advice</returns>
        /// <remarks>
        /// The method considers:
        /// - Local seasonal patterns for the specified city
        /// - Time of day for temperature variations
        /// - Weather conditions to provide relevant warnings (e.g., "Don't forget your umbrella" for rain)
        /// - Activity-appropriate advice based on weather conditions
        /// </remarks>
        public async Task<Weather> GetWeatherForDateAsync(string city, DateOnly date, TimeOnly time)
        {
            var prompt = $@"Provide weather information for {city} on {date:yyyy-MM-dd} at {time:HH:mm}.

Please include:
1. Temperature in Celsius (realistic for the location and time of year)
2. Brief weather description (sunny, cloudy, rainy, etc.)
3. A helpful one-sentence warning or advice based on the weather, for example:
   - For rain: ""Don't forget your umbrella!""
   - For hot weather: ""Stay hydrated and use sunscreen.""
   - For cold weather: ""Bring a warm jacket.""
   - For wind: ""Hold onto your hat, it's windy!""
   - For snow: ""Wear warm boots and watch for ice.""
   - For perfect weather: ""Perfect weather for outdoor activities!""

Format the response as JSON:
{{
  ""Degree"": number,
  ""Description"": ""weather description"",
  ""Warning"": ""helpful one-sentence warning or advice based on the weather"",
  ""Date"": ""{date:yyyy-MM-dd}""
}}";

            var jsonResponse = await GenerateContentAsync(prompt);
            try
            {
                jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                var weatherInfo = JsonSerializer.Deserialize<Weather>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (weatherInfo == null)
                    throw new GeminiAPIResponseParseException("Failed to parse weather information");

                weatherInfo.Date = date;
                return weatherInfo;
            }
            catch (JsonException ex)
            {
                throw new GeminiAPIResponseParseException("Failed to parse weather information", ex);
            }
        }

        /// <summary>
        /// Creates a customized travel route based on user preferences and requirements.
        /// </summary>
        /// <param name="destination">The target destination for the route</param>
        /// <param name="dayCount">Number of days for the trip</param>
        /// <param name="startDate">Start date of the trip for seasonal considerations</param>
        /// <param name="endDate">End date of the trip</param>
        /// <param name="priceLevel">Desired price level for venues and activities</param>
        /// <param name="accommodationPrefs">User's accommodation preferences (0-100 scale)</param>
        /// <param name="foodPrefs">User's food preferences and dietary restrictions (0-100 scale)</param>
        /// <param name="personalPrefs">User's personal activity and travel style preferences (0-100 scale)</param>
        /// <returns>A list of AnonymousDay objects containing the complete personalized itinerary</returns>
        /// <exception cref="ArgumentNullException">Thrown when destination is null or empty</exception>
        /// <exception cref="GeminiAPIResponseParseException">Thrown when the API response cannot be parsed into valid itinerary data</exception>
        public async Task<List<AnonymousDay>> CreateCustomRouteWithPreferences(
            string destination, 
            int dayCount, 
            DateOnly? startDate, 
            DateOnly? endDate, 
            PRICE_LEVEL? priceLevel,
            UserAccommodationPreferences accommodationPrefs,
            UserFoodPreferences foodPrefs,
            UserPersonalization personalPrefs)
        {
            var budgetContext = GetBudgetContext(priceLevel);
            var seasonalContext = GetSeasonalContext(startDate);

            var preferencesContext = $@"
### User Preferences Context:

ACCOMMODATION PREFERENCES (0-100 scale):
{(accommodationPrefs.LuxuryHotelPreference > 0 ? $"- Luxury Hotels: {accommodationPrefs.LuxuryHotelPreference}" : "")}
{(accommodationPrefs.BudgetHotelPreference > 0 ? $"- Budget Hotels: {accommodationPrefs.BudgetHotelPreference}" : "")}
{(accommodationPrefs.BoutiqueHotelPreference > 0 ? $"- Boutique Hotels: {accommodationPrefs.BoutiqueHotelPreference}" : "")}
{(accommodationPrefs.HostelPreference > 0 ? $"- Hostels: {accommodationPrefs.HostelPreference}" : "")}
{(accommodationPrefs.ApartmentPreference > 0 ? $"- Apartments: {accommodationPrefs.ApartmentPreference}" : "")}
{(accommodationPrefs.ResortPreference > 0 ? $"- Resorts: {accommodationPrefs.ResortPreference}" : "")}
{(accommodationPrefs.VillaPreference > 0 ? $"- Villas: {accommodationPrefs.VillaPreference}" : "")}
{(accommodationPrefs.GuestHousePreference > 0 ? $"- Guest Houses: {accommodationPrefs.GuestHousePreference}" : "")}
{(accommodationPrefs.CampingPreference > 0 ? $"- Camping: {accommodationPrefs.CampingPreference}" : "")}
{(accommodationPrefs.GlampingPreference > 0 ? $"- Glamping: {accommodationPrefs.GlampingPreference}" : "")}
{(accommodationPrefs.BedAndBreakfastPreference > 0 ? $"- Bed & Breakfast: {accommodationPrefs.BedAndBreakfastPreference}" : "")}
{(accommodationPrefs.AllInclusivePreference > 0 ? $"- All Inclusive: {accommodationPrefs.AllInclusivePreference}" : "")}
{(accommodationPrefs.SpaAndWellnessPreference > 0 ? $"- Spa & Wellness: {accommodationPrefs.SpaAndWellnessPreference}" : "")}
{(accommodationPrefs.PetFriendlyPreference > 0 ? $"- Pet Friendly: {accommodationPrefs.PetFriendlyPreference}" : "")}
{(accommodationPrefs.EcoFriendlyPreference > 0 ? $"- Eco Friendly: {accommodationPrefs.EcoFriendlyPreference}" : "")}
{(accommodationPrefs.CityCenterPreference > 0 ? $"- City Center Location: {accommodationPrefs.CityCenterPreference}" : "")}
{(accommodationPrefs.RemoteLocationPreference > 0 ? $"- Remote Location: {accommodationPrefs.RemoteLocationPreference}" : "")}
{(accommodationPrefs.FamilyFriendlyPreference > 0 ? $"- Family Friendly: {accommodationPrefs.FamilyFriendlyPreference}" : "")}
{(accommodationPrefs.AdultsOnlyPreference > 0 ? $"- Adults Only: {accommodationPrefs.AdultsOnlyPreference}" : "")}
{(accommodationPrefs.HomestayPreference > 0 ? $"- Homestay: {accommodationPrefs.HomestayPreference}" : "")}
{(accommodationPrefs.WaterfrontPreference > 0 ? $"- Waterfront: {accommodationPrefs.WaterfrontPreference}" : "")}
{(accommodationPrefs.HistoricalBuildingPreference > 0 ? $"- Historical Building: {accommodationPrefs.HistoricalBuildingPreference}" : "")}
{(accommodationPrefs.AirbnbPreference > 0 ? $"- Airbnb: {accommodationPrefs.AirbnbPreference}" : "")}
{(accommodationPrefs.CoLivingSpacePreference > 0 ? $"- Co-Living Space: {accommodationPrefs.CoLivingSpacePreference}" : "")}
{(accommodationPrefs.ExtendedStayPreference > 0 ? $"- Extended Stay: {accommodationPrefs.ExtendedStayPreference}" : "")}

FOOD PREFERENCES (0-100 scale):
{(foodPrefs.VeganPreference > 50 ? "- STRICT VEGAN - No animal products allowed" : "\n")}
{(foodPrefs.VegetarianPreference > 50 ? "- STRICT VEGETARIAN - No meat allowed" : "\n")}
{(foodPrefs.GlutenFreePreference > 50 ? "- STRICT GLUTEN-FREE required" : "\n")}
{(foodPrefs.HalalPreference > 50 ? "- STRICT HALAL food required" : "\n")}
{(foodPrefs.KosherPreference > 50 ? "- STRICT KOSHER food required" : "\n")}
{(foodPrefs.SeafoodPreference > 0 ? $"- Seafood: {foodPrefs.SeafoodPreference}" : "\n")}
{(foodPrefs.LocalCuisinePreference > 0 ? $"- Local Cuisine: {foodPrefs.LocalCuisinePreference}" : "\n")}
{(foodPrefs.FastFoodPreference > 0 ? $"- Fast Food: {foodPrefs.FastFoodPreference}" : "\n")}
{(foodPrefs.FinePreference > 0 ? $"- Fine Dining: {foodPrefs.FinePreference}" : "\n")}
{(foodPrefs.StreetFoodPreference > 0 ? $"- Street Food: {foodPrefs.StreetFoodPreference}" : "\n")}
{(foodPrefs.OrganicPreference > 0 ? $"- Organic Food: {foodPrefs.OrganicPreference}" : "\n")}
{(foodPrefs.BuffetPreference > 0 ? $"- Buffet: {foodPrefs.BuffetPreference}" : "\n")}
{(foodPrefs.FoodTruckPreference > 0 ? $"- Food Truck: {foodPrefs.FoodTruckPreference}" : "\n")}
{(foodPrefs.CafeteriaPreference > 0 ? $"- Cafeteria: {foodPrefs.CafeteriaPreference}" : "\n")}
{(foodPrefs.DeliveryPreference > 0 ? $"- Delivery: {foodPrefs.DeliveryPreference}" : "\n")}
{(foodPrefs.AllergiesPreference > 0 ? $"- Food Allergies: {foodPrefs.AllergiesPreference}" : "\n")}
{(foodPrefs.DairyFreePreference > 0 ? $"- Dairy Free: {foodPrefs.DairyFreePreference}" : "\n")}
{(foodPrefs.NutFreePreference > 0 ? $"- Nut Free: {foodPrefs.NutFreePreference}" : "\n")}
{(foodPrefs.SpicyPreference > 0 ? $"- Spicy Food: {foodPrefs.SpicyPreference}" : "\n")}
{(foodPrefs.SweetPreference > 0 ? $"- Sweet Food: {foodPrefs.SweetPreference}" : "\n")}
{(foodPrefs.SaltyPreference > 0 ? $"- Salty Food: {foodPrefs.SaltyPreference}" : "\n")}
{(foodPrefs.SourPreference > 0 ? $"- Sour Food: {foodPrefs.SourPreference}" : "\n")}
{(foodPrefs.BitterPreference > 0 ? $"- Bitter Food: {foodPrefs.BitterPreference}" : "\n")}
{(foodPrefs.UmamiPreference > 0 ? $"- Umami Food: {foodPrefs.UmamiPreference}" : "\n")}
{(foodPrefs.FusionPreference > 0 ? $"- Fusion Cuisine: {foodPrefs.FusionPreference}" : "\n")}

ACTIVITY PREFERENCES (0-100 scale):
{(personalPrefs.AdventurePreference > 0 ? $"- Adventure Activities: {personalPrefs.AdventurePreference}" : "\n")}
{(personalPrefs.RelaxationPreference > 0 ? $"- Relaxation Activities: {personalPrefs.RelaxationPreference}" : "\n")}
{(personalPrefs.CulturalPreference > 0 ? $"- Cultural Experiences: {personalPrefs.CulturalPreference}" : "\n")}
{(personalPrefs.NaturePreference > 0 ? $"- Nature Activities: {personalPrefs.NaturePreference}" : "\n")}
{(personalPrefs.UrbanPreference > 0 ? $"- Urban Experiences: {personalPrefs.UrbanPreference}" : "\n")}
{(personalPrefs.RuralPreference > 0 ? $"- Rural Experiences: {personalPrefs.RuralPreference}" : "\n")}
{(personalPrefs.LuxuryPreference > 0 ? $"- Luxury Experiences: {personalPrefs.LuxuryPreference}" : "\n")}
{(personalPrefs.BudgetPreference > 0 ? $"- Budget Activities: {personalPrefs.BudgetPreference}" : "\n")}
{(personalPrefs.SoloTravelPreference > 0 ? $"- Solo Travel: {personalPrefs.SoloTravelPreference}" : "\n")}
{(personalPrefs.GroupTravelPreference > 0 ? $"- Group Travel: {personalPrefs.GroupTravelPreference}" : "\n")}
{(personalPrefs.FamilyTravelPreference > 0 ? $"- Family Travel: {personalPrefs.FamilyTravelPreference}" : "\n")}
{(personalPrefs.CoupleTravelPreference > 0 ? $"- Couple Travel: {personalPrefs.CoupleTravelPreference}" : "\n")}
{(personalPrefs.BeachPreference > 0 ? $"- Beach Activities: {personalPrefs.BeachPreference}" : "\n")}
{(personalPrefs.MountainPreference > 0 ? $"- Mountain Activities: {personalPrefs.MountainPreference}" : "\n")}
{(personalPrefs.DesertPreference > 0 ? $"- Desert Activities: {personalPrefs.DesertPreference}" : "\n")}
{(personalPrefs.ForestPreference > 0 ? $"- Forest Activities: {personalPrefs.ForestPreference}" : "\n")}
{(personalPrefs.IslandPreference > 0 ? $"- Island Activities: {personalPrefs.IslandPreference}" : "\n")}
{(personalPrefs.LakePreference > 0 ? $"- Lake Activities: {personalPrefs.LakePreference}" : "\n")}
{(personalPrefs.RiverPreference > 0 ? $"- River Activities: {personalPrefs.RiverPreference}" : "\n")}
{(personalPrefs.WaterfallPreference > 0 ? $"- Waterfall Activities: {personalPrefs.WaterfallPreference}" : "\n")}
{(personalPrefs.CavePreference > 0 ? $"- Cave Exploration: {personalPrefs.CavePreference}" : "\n")}
{(personalPrefs.VolcanoPreference > 0 ? $"- Volcano Activities: {personalPrefs.VolcanoPreference}" : "\n")}
{(personalPrefs.GlacierPreference > 0 ? $"- Glacier Activities: {personalPrefs.GlacierPreference}" : "\n")}
{(personalPrefs.CanyonPreference > 0 ? $"- Canyon Activities: {personalPrefs.CanyonPreference}" : "\n")}
{(personalPrefs.ValleyPreference > 0 ? $"- Valley Activities: {personalPrefs.ValleyPreference}" : "\n")}";

            var prompt = $@"Create a detailed {dayCount}-day travel itinerary for {destination} based on specific user preferences. Format the response as a JSON object with the following structure:

{{
  ""AnonymousDayList"": [
        {{
      ""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""Specific hotel or accommodation name"",
      ""BreakfastPlaceName"": ""Specific restaurant or cafe name"",
      ""LunchPlaceName"": ""Specific restaurant name"",
      ""DinnerPlaceName"": ""Specific restaurant name"",
      ""FirstPlaceName"": ""First attraction or activity venue name"",
      ""SecondPlaceName"": ""Second attraction or activity venue name"",
      ""ThirdPlaceName"": ""Third attraction or activity venue name"",
      ""AfterDinnerPlaceName"": ""Evening venue or activity location name"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD""
        }}
    ]
}}

{preferencesContext}

### Budget Context:
{budgetContext}

### Season: 
{seasonalContext}

### CRITICAL RULES:
1. NEVER use NULL, null, N/A, or placeholder values
2. Every field MUST contain a real, existing venue name in {destination}
3. AccomodationPlaceName MUST be the same for all days - do not change hotels during the trip. But do not suggest same place in same route except its accomodation.
4. STRICTLY follow dietary restrictions when selecting restaurants
5. Ensure accommodation matches user's top preferences
6. Activities should align with personality preferences
7. All venues must be searchable on Google Maps
8. Never repeat restaurants in the itinerary
9. Balance between different areas of {destination}
10. Mix popular spots with hidden gems
11. Consider weather and seasonal factors
12. Four Seasons Hotel Istanbul At Sultanahmet IS FORBIDDEN DONT SUGGEST!

### VENUE SELECTION GUIDELINES:
1. Hotels: Use Accomodation preferences above.
2. Restaurants: Use Food preferences above.
3. Attractions: Balance iconic landmarks with local favorites use user personalizations above
4. Entertainment: Combine famous venues with unique local experiences use user personalizations above

### STRICTLY PROHIBITED:
1. Generic terms like ""Local market"", ""Street bazaar"", ""City center""
2. Descriptive locations like ""Old town square"", ""Shopping district"", ""Beach area""
3. Unnamed venues like ""Traditional restaurant"", ""Tourist spot"", ""Local cafe""

### REQUIRED VENUE TYPES:
1. Popular tourist attractions (e.g., ""Eiffel Tower"", ""Colosseum"", ""Taj Mahal"")
2. Well-known restaurants with actual names (e.g., ""Le Jules Verne"", ""Gordon Ramsay's Restaurant"")
3. Famous landmarks and museums (e.g., ""Louvre Museum"", ""British Museum"")
4. Established entertainment venues (e.g., ""Royal Albert Hall"", ""Sydney Opera House"")

### ADDITIONAL REQUIREMENTS:
1. Treat the last day like any other day - do not suggest airport or transport venues
2. Every venue must be searchable on Google Maps and have reviews/ratings
3. Focus on established venues that are popular with visitors
4. If unsure about a venue name, choose a more famous alternative
5. For price estimation, consider:
   - Hotel room cost per night
   - Average meal costs at each restaurant based on their price level
   - Entrance fees or tickets for attractions
   - Additional costs like drinks, snacks, or entertainment
   - Transportation between venues (approximate taxi/uber costs)";

            var jsonResponse = await GenerateContentAsync(prompt);
            
            try
            {
                jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                var response = JsonSerializer.Deserialize<GeminiResponseForAnonymousRoute>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (response?.AnonymousDayList == null || !response.AnonymousDayList.Any())
                    return await CreateCustomRouteWithPreferences(destination, dayCount, startDate, endDate, priceLevel, 
                        accommodationPrefs, foodPrefs, personalPrefs);

                foreach (var day in response.AnonymousDayList)
                {
                    if (string.IsNullOrWhiteSpace(day.AccomodationPlaceName) ||
                        string.IsNullOrWhiteSpace(day.BreakfastPlaceName) ||
                        string.IsNullOrWhiteSpace(day.LunchPlaceName) ||
                        string.IsNullOrWhiteSpace(day.DinnerPlaceName) ||
                        string.IsNullOrWhiteSpace(day.FirstPlaceName) ||
                        string.IsNullOrWhiteSpace(day.SecondPlaceName) ||
                        string.IsNullOrWhiteSpace(day.ThirdPlaceName) ||
                        string.IsNullOrWhiteSpace(day.AfterDinnerPlaceName) ||
                        string.IsNullOrWhiteSpace(day.DayDescription))
                    {
                        return await CreateCustomRouteWithPreferences(destination, dayCount, startDate, endDate, priceLevel,
                            accommodationPrefs, foodPrefs, personalPrefs);
                    }
                }

                return response.AnonymousDayList;
            }
            catch (JsonException)
            {
                return await CreateCustomRouteWithPreferences(destination, dayCount, startDate, endDate, priceLevel,
                    accommodationPrefs, foodPrefs, personalPrefs);
            }
        }

        /// <summary>
        /// Gets the budget context description based on the specified price level.
        /// </summary>
        /// <param name="priceLevel">The price level enum value</param>
        /// <returns>A detailed string describing budget considerations and recommendations for the specified price level</returns>
        private string GetBudgetContext(PRICE_LEVEL? priceLevel)
        {
            return priceLevel switch
            {
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => @"Budget-Friendly Focus:
- Prioritize affordable accommodations (hostels, budget hotels, guesthouses)
- Select reasonably priced restaurants and cafes
- Include free or low-cost attractions and activities
- Suggest budget-friendly transportation options
- Total daily budget (excluding accommodation): $50-100 per person",

                PRICE_LEVEL.PRICE_LEVEL_MODERATE => @"Moderate Budget Focus:
- Mid-range hotels and boutique accommodations
- Mix of casual and upscale dining options
- Balance of paid attractions and free activities
- Comfortable transportation options
- Total daily budget (excluding accommodation): $100-200 per person",

                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => @"Luxury Focus:
- 4-5 star hotels and luxury accommodations
- High-end restaurants and fine dining experiences
- Premium attractions and exclusive activities
- Private transportation options
- Total daily budget (excluding accommodation): $200-500 per person",

                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => @"Ultra-Luxury Focus:
- 5-star and boutique luxury hotels
- Michelin-starred restaurants and exclusive dining experiences
- VIP access to attractions and private tours
- Luxury car services and private transfers
- High-end shopping recommendations
- Exclusive nightlife venues and entertainment
- Spa and wellness experiences at premium establishments
- Total daily budget (excluding accommodation): $500+ per person",

                _ => @"Budget-Friendly Focus:
- Prioritize affordable accommodations (hostels, budget hotels, guesthouses)
- Select reasonably priced restaurants and cafes
- Include free or low-cost attractions and activities
- Suggest budget-friendly transportation options
- Total daily budget (excluding accommodation): $50-100 per person"
            };
        }

        public async Task<List<string>> AnalyzePlacePreferencesAsync(string placeName, string placeType, string placeDescription, List<string> availablePreferences)
        {
            var prompt = $@"Analyze this place and determine which user preferences should be updated based on its characteristics.

Place Information:
- Name: {placeName}
- Type: {placeType}
- Description: {placeDescription}

Available Preferences to Update:
{string.Join("\n", availablePreferences.Select(p => $"- {p}"))}

Rules:
1. Return a list of preferences that should be updated based on the place's characteristics
2. Only select preferences that are strongly relevant to the place
3. Consider the place type, features, and overall experience
4. Do not include preferences that don't clearly match the place's characteristics
5. Return the response as a JSON array of preference names
6. If a hotel is recommended where there should be a restaurant, consider it as the hotel's restaurant.

Example Response Format:
[""PreferenceOne"", ""PreferenceTwo"", ""PreferenceThree""]

IMPORTANT:
- Return ONLY the JSON array, no additional text
- Only include preferences from the provided list
- Select preferences that have a clear connection to the place
- Do not include preferences just because they might be slightly relevant";

            try
            {
                var jsonResponse = await GenerateContentAsync(prompt);
                jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                if (!jsonResponse.StartsWith("[") || !jsonResponse.EndsWith("]"))
                {
                    var startIndex = jsonResponse.IndexOf('[');
                    var endIndex = jsonResponse.LastIndexOf(']');
                    if (startIndex >= 0 && endIndex >= 0)
                    {
                        jsonResponse = jsonResponse.Substring(startIndex, endIndex - startIndex + 1);
                    }
                    else
                    {
                        throw new GeminiAPIResponseParseException("Invalid response format");
                    }
                }

                var preferences = JsonSerializer.Deserialize<List<string>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (preferences == null || !preferences.Any())
                {
                    return null;
                    throw new GeminiAPIResponseParseException("No preferences returned from analysis");
                }

                preferences = preferences
                    .Where(p => availablePreferences.Contains(p, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (!preferences.Any())
                {
                    throw new GeminiAPIResponseParseException("No valid preferences found in response");
                }

                return preferences;
            }
            catch (JsonException ex)
            {
                throw new GeminiAPIResponseParseException("Failed to parse preferences from response", ex);
            }
            catch (Exception ex) when (ex is not BaseException)
            {
                throw new GeminiAIServiceException("Failed to analyze place preferences", ex);
            }
        }

        public async Task<List<NewCustomRouteDTO>> CreateCustomRoute(string destination, int dayCount, DateOnly? startDate , DateOnly? endDate, PRICE_LEVEL? priceLevel,UserAccommodationPreferences accommodationPrefs,
            UserFoodPreferences foodPrefs,
            UserPersonalization personalPrefs)
        {
            var budgetContext = GetBudgetContext(priceLevel);
            var seasonalContext = GetSeasonalContext(startDate);

            var accommodationPreferencesList = typeof(UserAccommodationPreferences)
        .GetProperties()
        .Where(p => p.PropertyType == typeof(int?) &&
                    p.Name.EndsWith("Preference") &&
                    (int?)p.GetValue(accommodationPrefs) == 100)
        .Select(p => p.Name)
        .ToList();

            var foodPreferencesList = typeof(UserFoodPreferences)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(int?) &&
                            p.Name.EndsWith("Preference") &&
                            (int?)p.GetValue(foodPrefs) == 100)
                .Select(p => p.Name)
                .ToList();

            var personalPreferencesList = typeof(UserPersonalization)
                .GetProperties()
                .Where(p => p.PropertyType == typeof(int?) &&
                            p.Name.EndsWith("Preference") &&
                            (int?)p.GetValue(personalPrefs) == 100)
                .Select(p => p.Name)
                .ToList();

            var diff = (dayCount * 4) - foodPreferencesList.Count;
            for(int i = 0; i < diff; i++) 
            {
                foodPreferencesList.Add(foodPreferencesList[i]);
            }

            diff = (dayCount * 4) - personalPreferencesList.Count;
            for(int i = 0;i < diff; i++) 
            {
                personalPreferencesList.Add(personalPreferencesList[i]);
            }

            string prompt;

            if (dayCount == 1)
            {
                prompt = $@"I want you to create a custom travel route for 1 day in {destination}.This route will be in English and every place in this route must be unique and different from others. Your budget is {priceLevel} and your budget context is : {budgetContext}.Also I want you to consider the season and season is {seasonalContext}. Every component in these route must be a real place in {destination} and cant be null , NULL or N/A.Remember this is a touristic route so think like a tourist.And please dont recommend airport for the last day. here is the json format

{{
  ""AnonymousDayList"": [
        {{
      ""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[0]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[1]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[2]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[0]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[1]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[2]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[3]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
        
        }}
    ]
}}
                    
                    
";
            }

            else if (dayCount == 2)
            {
                prompt = $@"I want you to create a custom travel route for 1 day in {destination}.This route will be in English and every place in this route must be unique and different from others. Your budget is {priceLevel} and your budget context is : {budgetContext}.Also I want you to consider the season and season is {seasonalContext}. Every component in these route must be a real place in {destination} and cant be null , NULL or N/A.Remember this is a touristic route so think like a tourist.And please dont recommend airport for the last day. here is the json format

{{
  ""AnonymousDayList"": [
        {{
      ""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[0]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[1]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[2]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[0]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[1]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[2]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[3]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
        
        }},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[4]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[5]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[6]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[3]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[4]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[5]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[7]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
}}

    ]
}}
                    
                    
";
            }

            else if (dayCount == 3)
            {
                prompt = $@"I want you to create a custom travel route for 1 day in {destination}.This route will be in English and every place in this route must be unique and different from others. Your budget is {priceLevel} and your budget context is : {budgetContext}.Also I want you to consider the season and season is {seasonalContext}. Every component in these route must be a real place in {destination} and cant be null , NULL or N/A.Remember this is a touristic route so think like a tourist.And please dont recommend airport for the last day. here is the json format

{{
  ""AnonymousDayList"": [
        {{
      ""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[0]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[1]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[2]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[0]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[1]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[2]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[3]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
        
        }},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[4]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[5]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[6]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[3]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[4]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[5]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[7]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[8]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[9]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[10]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[6]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[7]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[8]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[11]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
}}

    ]
}}
                    
                    
";
            }

            else if (dayCount == 4)
            {
                prompt = $@"I want you to create a custom travel route for 1 day in {destination}.This route will be in English and every place in this route must be unique and different from others. Your budget is {priceLevel} and your budget context is : {budgetContext}.Also I want you to consider the season and season is {seasonalContext}. Every component in these route must be a real place in {destination} and cant be null , NULL or N/A.Remember this is a touristic route so think like a tourist.And please dont recommend airport for the last day. here is the json format

{{
  ""AnonymousDayList"": [
        {{
      ""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[0]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[1]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[2]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[0]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[1]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[2]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[3]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
        
        }},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[4]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[5]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[6]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[3]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[4]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[5]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[7]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[8]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[9]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[10]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[6]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[7]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[8]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[11]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
}},{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[12]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[13]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[14]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[9]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[10]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[11]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[15]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for 3 days later of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for 3 days later of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for 3 days later of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
}}

    ]
}}
                    
                    
";
            }

            else 
            {
                prompt = $@"I want you to create a custom travel route for 1 day in {destination}.This route will be in English and every place in this route must be unique and different from others. Your budget is {priceLevel} and your budget context is : {budgetContext}.Also I want you to consider the season and season is {seasonalContext}. Every component in these route must be a real place in {destination} and cant be null , NULL or N/A.Remember this is a touristic route so think like a tourist.And please dont recommend airport for the last day. here is the json format

{{
  ""AnonymousDayList"": [
        {{
      ""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[0]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[1]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[2]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[0]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[1]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[2]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[3]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
        
        }},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[4]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[5]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[6]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[3]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[4]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[5]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[7]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for next day of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[8]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[9]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[10]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[6]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[7]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[8]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[11]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for 2 days later of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
}},{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[12]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[13]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[14]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[9]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[10]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[11]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[15]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for 3 days later of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for 3 days later of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for 3 days later of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
}},
{{
""DayDescription"": ""Detailed description of the day's activities and flow"",
      ""AccomodationPlaceName"": ""{accommodationPreferencesList[0]} Hotel name.(only hotel name and star e.g X hotel - 5 star)(must be same for all route days)"",
      ""BreakfastPlaceName"": ""{foodPreferencesList[16]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""LunchPlaceName"": ""{foodPreferencesList[17]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""DinnerPlaceName"": ""{foodPreferencesList[18]} Specific restaurant or cafe name, be aware dont recommend the same place again"",
      ""FirstPlaceName"": ""{personalPreferencesList[12]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""SecondPlaceName"": ""{personalPreferencesList[13]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""ThirdPlaceName"": ""{personalPreferencesList[14]} First attraction or activity venue name, be aware dont recommend the same place again"",
      ""AfterDinnerPlaceName"": ""{foodPreferencesList[19]} Evening venue or activity location name,, be aware dont recommend the same place again"",
      ""ApproxPrice"": ""Estimated total cost for the day in USD"",
      ""WeatherForMorning"" : {{
        ""Degree"" : ""int celcius degree for 4 days later of {startDate} morning"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNoon"" : {{
        ""Degree"" : ""int celcius degree for 4 days later of {startDate} noon"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}},
        ""WeatherForNight"" : {{
        ""Degree"" : ""int celcius degree for 4 days later of {startDate} night"",
        ""Description"" : ""string short 5-6 words weather description"",
        ""Warning"" : ""string short 5-6 words weather warning e.g Don't forget your umbrella!"",
        ""Date"" : ""DateOnly date""
}}
}}

    ]
}}
                    
                    
";
            }

            var jsonResponse = await GenerateContentAsync(prompt);

           
                jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();

                var response = JsonSerializer.Deserialize<List<NewCustomRouteDTO>>(jsonResponse, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });


                return response;    
            
        }

    }

        

        
}
 