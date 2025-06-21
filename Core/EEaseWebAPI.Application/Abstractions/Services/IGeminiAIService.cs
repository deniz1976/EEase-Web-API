using EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Domain.Entities.Identity;
using System.Threading.Tasks;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Application.DTOs.Route.NewCustomRoute;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGeminiAIService
    {
        /// <summary>
        /// Analyzes a user's message to extract their travel preferences and personalizations.
        /// Uses natural language processing to understand and categorize user preferences.
        /// </summary>
        /// <param name="message">The user's message containing their travel preferences and requirements</param>
        /// <returns>A tuple containing three preference objects:
        /// - UserAccommodationPreferences: Preferences for lodging and accommodation
        /// - UserFoodPreferences: Dietary requirements and food preferences
        /// - UserPersonalization: General travel style and activity preferences</returns>
        /// <remarks>
        /// The method processes preferences for:
        /// - Accommodation: Hotel types, amenities, location preferences
        /// - Food: Cuisine types, dietary restrictions, dining styles
        /// - Personalization: Activity types, travel pace, budget level
        /// </remarks>
        Task<(UserAccommodationPreferences, UserFoodPreferences, UserPersonalization)> GetUserPreferencesFromMessage(string message);

        /// <summary>
        /// Generates content using the Gemini AI model based on the provided prompt.
        /// Handles communication with the Gemini AI API and processes the response.
        /// </summary>
        /// <param name="prompt">The input prompt for content generation</param>
        /// <returns>The generated content as a string</returns>
        /// <remarks>
        /// This method:
        /// - Validates and formats the input prompt
        /// - Manages API communication with Gemini AI
        /// - Handles rate limiting and retries
        /// - Processes and validates the response
        /// </remarks>
        Task<string> GenerateContentAsync(string prompt);

        /// <summary>
        /// Creates a detailed travel itinerary for anonymous users using Gemini AI.
        /// Generates a comprehensive day-by-day plan based on specified parameters.
        /// </summary>
        /// <param name="destination">The target destination for the travel itinerary</param>
        /// <param name="dayCount">Number of days for the trip</param>
        /// <param name="startDate">Start date of the trip for seasonal considerations</param>
        /// <param name="endDate">End date of the trip</param>
        /// <param name="_PRICE_LEVEL">Desired price level for venues and activities</param>
        /// <returns>A list of AnonymousDay objects containing the complete itinerary</returns>
        /// <remarks>
        /// The itinerary includes:
        /// - Daily accommodations and meals
        /// - Tourist attractions and activities
        /// - Time-appropriate venue suggestions
        /// - Weather-conscious planning
        /// - Budget-aligned recommendations
        /// </remarks>
        Task<List<EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin.AnonymousDay>> CreateRouteAnonymous(string? destination, int dayCount, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL);

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
        Task<Weather> GetWeatherForDateAsync(string city, DateOnly date, TimeOnly time);

        Task<List<AnonymousDay>> CreateCustomRouteWithPreferences(
            string destination,
            int dayCount,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            UserAccommodationPreferences accommodationPrefs,
            UserFoodPreferences foodPrefs,
            UserPersonalization personalPrefs);

        /// <summary>
        /// Analyzes a place or restaurant to determine which user preferences should be updated.
        /// </summary>
        /// <param name="placeName">Name of the place or restaurant</param>
        /// <param name="placeType">Type of the place (e.g., restaurant, museum, park)</param>
        /// <param name="placeDescription">Detailed description of the place including its characteristics</param>
        /// <param name="availablePreferences">List of available preference fields that can be updated</param>
        /// <returns>List of preference names that should be updated based on the place characteristics</returns>
        Task<List<string>> AnalyzePlacePreferencesAsync(string placeName, string placeType, string placeDescription, List<string> availablePreferences);

        Task<List<NewCustomRouteDTO>> CreateCustomRoute(string destination, int dayCount, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? priceLevel, UserAccommodationPreferences accommodationPrefs,
            UserFoodPreferences foodPrefs,
            UserPersonalization personalPrefs);
    }
} 