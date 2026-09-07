using EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Domain.Entities.Identity;
using System.Threading.Tasks;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Application.DTOs.Route.NewCustomRoute;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGeminiAIService
    {
        Task<(UserAccommodationPreferences, UserFoodPreferences, UserPersonalization)> GetUserPreferencesFromMessage(string message);

        Task<string> GenerateContentAsync(string prompt);

        Task<List<EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin.AnonymousDay>> CreateRouteAnonymous(string? destination, int dayCount, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? _PRICE_LEVEL);

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

        Task<List<string>> AnalyzePlacePreferencesAsync(string placeName, string placeType, string placeDescription, List<string> availablePreferences);

        Task<List<NewCustomRouteDTO>> CreateCustomRoute(string destination, int dayCount, DateOnly? startDate, DateOnly? endDate, PRICE_LEVEL? priceLevel, UserAccommodationPreferences accommodationPrefs,
            UserFoodPreferences foodPrefs,
            UserPersonalization personalPrefs);
    }
}
