using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// The two things the product asks Gemini for outside route enrichment: reading
    /// preferences out of a sentence the user wrote, and deciding which preferences a place
    /// matches. Route enrichment talks to <see cref="IGeminiApiClient"/> directly.
    /// </summary>
    public interface IGeminiAIService
    {
        Task<(UserAccommodationPreferences Accommodation, UserFoodPreferences Food, UserPersonalization Personalization)>
            GetUserPreferencesFromMessage(string message, CancellationToken cancellationToken = default);

        Task<List<string>> AnalyzePlacePreferencesAsync(
            string placeName,
            string placeType,
            string placeDescription,
            List<string> availablePreferences,
            CancellationToken cancellationToken = default);
    }
}
