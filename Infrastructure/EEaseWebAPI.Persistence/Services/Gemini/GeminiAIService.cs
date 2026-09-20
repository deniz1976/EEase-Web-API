using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.MapEntities.GeminiAI;
using EEaseWebAPI.Domain.Entities.Identity;
using System.Text.Json;

namespace EEaseWebAPI.Persistence.Services.Gemini
{
    public sealed class GeminiAIService : IGeminiAIService
    {
        private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNameCaseInsensitive = true };

        private readonly IGeminiApiClient _apiClient;

        public GeminiAIService(IGeminiApiClient apiClient)
        {
            _apiClient = apiClient;
        }

        public async Task<(UserAccommodationPreferences Accommodation, UserFoodPreferences Food, UserPersonalization Personalization)>
            GetUserPreferencesFromMessage(string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(message))
            {
                throw new GeminiInvalidMessageException("The message cannot be empty or whitespace.");
            }

            var responseText = await _apiClient.GenerateContentAsync(
                GeminiPrompts.UserPreferences(message), expectJson: true, cancellationToken);

            var preferences = Parse<PreferencesResponse>(responseText, "preferences")
                ?? throw new GeminiAPIResponseParseException("Could not parse the preferences response.");

            return (
                preferences.AccommodationPreferences ?? new UserAccommodationPreferences(),
                preferences.FoodPreferences ?? new UserFoodPreferences(),
                preferences.Personalization ?? new UserPersonalization()
            );
        }

        public async Task<List<string>> AnalyzePlacePreferencesAsync(
            string placeName,
            string placeType,
            string placeDescription,
            List<string> availablePreferences,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _apiClient.GenerateContentAsync(
                    GeminiPrompts.PlacePreferences(placeName, placeType, placeDescription, availablePreferences),
                    cancellationToken: cancellationToken);

                var json = ExtractJsonArray(Strip(response));

                var preferences = (Parse<List<string>>(json, "place preferences") ?? new List<string>())
                    .Where(preference => availablePreferences.Contains(preference, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                if (preferences.Count == 0)
                {
                    throw new GeminiAPIResponseParseException("No valid preferences found in response");
                }

                return preferences;
            }
            catch (Exception exception) when (exception is not BaseException and not OperationCanceledException)
            {
                throw new GeminiAIServiceException("Failed to analyze place preferences", exception);
            }
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
            if (response.StartsWith('[') && response.EndsWith(']'))
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
