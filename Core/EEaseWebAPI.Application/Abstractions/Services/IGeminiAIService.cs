using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
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
