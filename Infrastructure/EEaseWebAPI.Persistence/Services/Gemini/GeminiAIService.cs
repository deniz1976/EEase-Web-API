using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Application.DTOs.Route.NewCustomRoute;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.MapEntities.GeminiAI;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using System.Reflection;
using System.Text.Json;

namespace EEaseWebAPI.Persistence.Services.Gemini
{
    public class GeminiAIService : IGeminiAIService
    {
        private const int MaxRouteAttempts = 3;
        private const int MealsPerDay = 4;
        private const int PlacesPerDay = 3;
        private const int StrongPreferenceThreshold = 60;

        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IGeminiApiClient _apiClient;

        public GeminiAIService(IGeminiApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public Task<string> GenerateContentAsync(string prompt)
        {
            return _apiClient.GenerateContentAsync(prompt);
        }

        public async Task<List<AnonymousDay>> CreateRouteAnonymous(
            string? destination, int dayCount, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? priceLevel = null)
        {
            if (string.IsNullOrWhiteSpace(destination))
                throw new ArgumentNullException(nameof(destination));

            var budgetContext = priceLevel switch
            {
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE => "Budget-Friendly",
                PRICE_LEVEL.PRICE_LEVEL_MODERATE => "Moderate Budget",
                PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE => "Luxury Focus",
                PRICE_LEVEL.PRICE_LEVEL_VERY_EXPENSIVE => "Ultra-Luxury",
                _ => "Budget-Friendly"
            };

            var prompt = GeminiPrompts.AnonymousRoute(
                destination, dayCount, budgetContext, GeminiPrompts.SeasonalContext(startDate));

            return await RequestDaysAsync(prompt, RejectsPlaceholders);
        }

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
            var prompt = GeminiPrompts.CustomRouteWithPreferences(
                destination,
                dayCount,
                GeminiPrompts.BudgetContext(priceLevel),
                GeminiPrompts.SeasonalContext(startDate),
                GeminiPrompts.PreferencesContext(accommodationPrefs, foodPrefs, personalPrefs));

            return await RequestDaysAsync(prompt, IsComplete);
        }

        public async Task<(UserAccommodationPreferences, UserFoodPreferences, UserPersonalization)> GetUserPreferencesFromMessage(string message)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new GeminiInvalidMessageException("The message cannot be empty or whitespace.");
            }

            var responseText = await _apiClient.GenerateContentAsync(
                GeminiPrompts.UserPreferences(message), expectJson: true);

            var preferences = Parse<PreferencesResponse>(responseText, "preferences")
                ?? throw new GeminiAPIResponseParseException("Could not parse the preferences response.");

            return (
                preferences.AccommodationPreferences ?? new UserAccommodationPreferences(),
                preferences.FoodPreferences ?? new UserFoodPreferences(),
                preferences.Personalization ?? new UserPersonalization()
            );
        }

        public async Task<Weather> GetWeatherForDateAsync(string city, DateOnly date, TimeOnly time)
        {
            var response = await GenerateContentAsync(GeminiPrompts.Weather(city, date, time));

            var weather = Parse<Weather>(response, "weather information")
                ?? throw new GeminiAPIResponseParseException("Failed to parse weather information");

            weather.Date = date;
            return weather;
        }

        public async Task<List<string>> AnalyzePlacePreferencesAsync(
            string placeName, string placeType, string placeDescription, List<string> availablePreferences)
        {
            try
            {
                var response = await GenerateContentAsync(
                    GeminiPrompts.PlacePreferences(placeName, placeType, placeDescription, availablePreferences));

                var json = ExtractJsonArray(Strip(response));

                var preferences = Parse<List<string>>(json, "place preferences") ?? new List<string>();

                preferences = preferences
                    .Where(preference => availablePreferences.Contains(preference, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (preferences.Count == 0)
                {
                    throw new GeminiAPIResponseParseException("No valid preferences found in response");
                }

                return preferences;
            }
            catch (Exception exception) when (exception is not BaseException)
            {
                throw new GeminiAIServiceException("Failed to analyze place preferences", exception);
            }
        }

        public async Task<List<NewCustomRouteDTO>> CreateCustomRoute(
            string destination,
            int dayCount,
            DateOnly? startDate,
            DateOnly? endDate,
            PRICE_LEVEL? priceLevel,
            UserAccommodationPreferences accommodationPrefs,
            UserFoodPreferences foodPrefs,
            UserPersonalization personalPrefs)
        {
            var accommodation = TopPreferences(accommodationPrefs, 1, "Comfortable");
            var food = TopPreferences(foodPrefs, dayCount * MealsPerDay, "Popular");
            var personal = TopPreferences(personalPrefs, dayCount * PlacesPerDay, "Popular");

            var prompt = GeminiPrompts.CustomRoute(
                destination,
                dayCount,
                priceLevel,
                startDate,
                GeminiPrompts.BudgetContext(priceLevel),
                GeminiPrompts.SeasonalContext(startDate),
                accommodation,
                food,
                personal);

            var response = await GenerateContentAsync(prompt);

            return Parse<List<NewCustomRouteDTO>>(response, "custom route") ?? new List<NewCustomRouteDTO>();
        }

        private async Task<List<AnonymousDay>> RequestDaysAsync(string prompt, Func<AnonymousDay, bool> isUsable)
        {
            for (var attempt = 1; attempt <= MaxRouteAttempts; attempt++)
            {
                var response = await GenerateContentAsync(prompt);

                List<AnonymousDay>? days;

                try
                {
                    days = Parse<GeminiResponseForAnonymousRoute>(response, "itinerary")?.AnonymousDayList;
                }
                catch (GeminiAPIResponseParseException) when (attempt < MaxRouteAttempts)
                {
                    continue;
                }

                if (days != null && days.Count > 0 && days.All(isUsable))
                {
                    return days;
                }
            }

            throw new GeminiAPIResponseParseException(
                $"The itinerary could not be generated after {MaxRouteAttempts} attempts.");
        }

        private static bool IsComplete(AnonymousDay day) =>
            PlaceNames(day).All(name => !string.IsNullOrWhiteSpace(name));

        private static bool RejectsPlaceholders(AnonymousDay day) =>
            IsComplete(day) &&
            PlaceNames(day).All(name =>
                !name!.Equals("NULL", StringComparison.OrdinalIgnoreCase) &&
                !name.Equals("N/A", StringComparison.OrdinalIgnoreCase));

        private static IEnumerable<string?> PlaceNames(AnonymousDay day)
        {
            yield return day.DayDescription;
            yield return day.AccomodationPlaceName;
            yield return day.BreakfastPlaceName;
            yield return day.LunchPlaceName;
            yield return day.DinnerPlaceName;
            yield return day.FirstPlaceName;
            yield return day.SecondPlaceName;
            yield return day.ThirdPlaceName;
            yield return day.AfterDinnerPlaceName;
        }

        private static List<string> TopPreferences(object preferences, int count, string fallback)
        {
            var names = preferences.GetType()
                .GetProperties()
                .Where(IsPreference)
                .Select(property => (property.Name, Score: (int?)property.GetValue(preferences) ?? 0))
                .Where(candidate => candidate.Score >= StrongPreferenceThreshold)
                .OrderByDescending(candidate => candidate.Score)
                .Select(candidate => candidate.Name)
                .ToList();

            if (names.Count == 0)
            {
                names.Add(fallback);
            }

            for (var index = 0; names.Count < count; index++)
            {
                names.Add(names[index]);
            }

            return names;

            static bool IsPreference(PropertyInfo property) =>
                property.PropertyType == typeof(int?) &&
                property.Name.EndsWith("Preference");
        }

        private static T? Parse<T>(string response, string what)
        {
            try
            {
                return JsonSerializer.Deserialize<T>(Strip(response), JsonOptions);
            }
            catch (JsonException exception)
            {
                throw new GeminiAPIResponseParseException($"Could not parse the {what} response.", exception);
            }
        }

        private static string Strip(string response) =>
            response.Replace("```json", "").Replace("```", "").Trim();

        private static string ExtractJsonArray(string response)
        {
            if (response.StartsWith("[") && response.EndsWith("]"))
            {
                return response;
            }

            var start = response.IndexOf('[');
            var end = response.LastIndexOf(']');

            if (start < 0 || end < start)
            {
                throw new GeminiAPIResponseParseException("Invalid response format");
            }

            return response.Substring(start, end - start + 1);
        }
    }
}
