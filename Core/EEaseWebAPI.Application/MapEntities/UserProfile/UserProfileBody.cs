using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Domain.Enums;

namespace EEaseWebAPI.Application.MapEntities.UserProfile
{
    public class UserProfileBody
    {
        public string? Id { get; set; }

        public string? Username { get; set; }

        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Bio { get; set; }

        public string? PhotoPath { get; set; }

        public string? Gender { get; set; }

        public string? Country { get; set; }

        public string? ErrorMessage { get; set; }

        public bool CanSendFriendRequest { get; set; }

        public bool IsFriend { get; set; }

        public FriendRequestStatus? FriendRequestStatus { get; set; }

        public ProfileVisibilityStatus VisibilityStatus { get; set; }

        public List<PreferenceDetail>? PersonalizationPreferences { get; set; }

        public List<PreferenceDetail>? FoodPreferences { get; set; }

        public List<PreferenceDetail>? AccommodationPreferences { get; set; }
    }
}
