using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class StandardRoute : BaseEntity
    {
        public string? Name { get; set; }

        public string? City { get; set; }

        public AppUser? User { get; set; }

        public string? UserId { get; set; }

        public List<AppUser>? LikedUsers { get; set; }

        public int? Days { get; set; }

        public int? LikeCount { get; set; }

        public string? Currency { get; set; } = "TRY";

        /// <summary>
        /// A route always has its days: an empty list means one was never built, not that
        /// the days are unknown.
        /// </summary>
        public List<TravelDay> TravelDays { get; set; } = new();

        public UserFoodPreferences? UserFoodPreferences { get; set; }

        public UserPersonalization? UserPersonalization { get; set; }

        public UserAccommodationPreferences? UserAccommodationPreferences { get; set; }

        public int? Status { get; set; }
    }
}
