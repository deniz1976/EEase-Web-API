using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Collects the stored preferences of everyone travelling together, so a route can be
    /// planned for a user and the friends they invited.
    /// </summary>
    public interface ITravellerPreferenceCollector
    {
        Task<IReadOnlyList<(UserAccommodationPreferences? Accommodation, UserFoodPreferences? Food, UserPersonalization? Personalization)>>
            CollectAsync(AppUser user, IReadOnlyList<string>? friendUsernames, CancellationToken cancellationToken = default);
    }
}
