using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface ITravellerPreferenceCollector
    {
        Task<IReadOnlyList<(UserAccommodationPreferences? Accommodation, UserFoodPreferences? Food, UserPersonalization? Personalization)>>
            CollectAsync(AppUser user, IReadOnlyList<string>? friendUsernames, CancellationToken cancellationToken = default);
    }
}
