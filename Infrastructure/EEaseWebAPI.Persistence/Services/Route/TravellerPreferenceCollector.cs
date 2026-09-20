using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class TravellerPreferenceCollector : ITravellerPreferenceCollector
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IFriendshipService _friendshipService;
        private readonly EEaseAPIDbContext _context;

        public TravellerPreferenceCollector(
            UserManager<AppUser> userManager,
            IFriendshipService friendshipService,
            EEaseAPIDbContext context)
        {
            _userManager = userManager;
            _friendshipService = friendshipService;
            _context = context;
        }

        public async Task<IReadOnlyList<(UserAccommodationPreferences? Accommodation, UserFoodPreferences? Food, UserPersonalization? Personalization)>>
            CollectAsync(AppUser user, IReadOnlyList<string>? friendUsernames, CancellationToken cancellationToken = default)
        {
            var travellerIds = await ResolveTravellerIdsAsync(user, friendUsernames);

            var accommodation = await _context.Set<UserAccommodationPreferences>()
                .Where(preference => preference.UserId != null && travellerIds.Contains(preference.UserId))
                .ToDictionaryAsync(preference => preference.UserId!, cancellationToken);

            var food = await _context.Set<UserFoodPreferences>()
                .Where(preference => preference.UserId != null && travellerIds.Contains(preference.UserId))
                .ToDictionaryAsync(preference => preference.UserId!, cancellationToken);

            var personalization = await _context.Set<UserPersonalization>()
                .Where(preference => preference.UserId != null && travellerIds.Contains(preference.UserId))
                .ToDictionaryAsync(preference => preference.UserId!, cancellationToken);

            var collected = new List<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>();

            foreach (var id in travellerIds)
            {
                accommodation.TryGetValue(id, out var accommodationPreference);
                food.TryGetValue(id, out var foodPreference);
                personalization.TryGetValue(id, out var personalizationPreference);

                // A traveller who filled in only some of the three questionnaires still counts;
                // dropping them would silently plan the trip as if they were not coming.
                if (accommodationPreference is not null ||
                    foodPreference is not null ||
                    personalizationPreference is not null)
                {
                    collected.Add((accommodationPreference, foodPreference, personalizationPreference));
                }
            }

            return collected;
        }

        private async Task<List<string>> ResolveTravellerIdsAsync(AppUser user, IReadOnlyList<string>? friendUsernames)
        {
            var travellers = new List<AppUser> { user };

            foreach (var friendUsername in friendUsernames ?? Array.Empty<string>())
            {
                if (string.IsNullOrWhiteSpace(friendUsername) ||
                    travellers.Any(traveller => string.Equals(traveller.UserName, friendUsername, StringComparison.OrdinalIgnoreCase)))
                {
                    continue;
                }

                var friend = await _userManager.FindByNameAsync(friendUsername);

                if (friend is not null && await _friendshipService.AreFriendsAsync(user.UserName!, friendUsername))
                {
                    travellers.Add(friend);
                }
            }

            return travellers.Select(traveller => traveller.Id).ToList();
        }
    }
}
