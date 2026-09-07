using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IPreferenceProfileBuilder
    {
        PreferenceProfile Build(
            UserAccommodationPreferences? accommodation,
            UserFoodPreferences? food,
            UserPersonalization? personalization);

        PreferenceProfile Build(
            IEnumerable<(UserAccommodationPreferences? Accommodation, UserFoodPreferences? Food, UserPersonalization? Personalization)> preferences);
    }
}
