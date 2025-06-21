using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

// Status 0 = private to everyone,
// Status 1 = open to friends,
// Status 2 = open to everyone.

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class StandardRoute : BaseEntity
    {
        public Guid Id { get; set; }
        public string? City { get; set; }
        public AppUser? User { get; set; }
        public List<AppUser>? LikedUsers { get; set; }
        public string? name { get; set; }
        public string? UserId { get; set; }
        public int? Days { get; set; }
        public int? LikeCount { get; set; }
        public string? Currency { get; set; } = "TRY";
        public List<TravelDay>? TravelDays { get; set; }
        public UserFoodPreferences? UserFoodPreferences { get; set; } = null;
        public UserPersonalization? UserPersonalization { get; set; } = null;
        public UserAccommodationPreferences? UserAccommodationPreferences { get; set; } = null;
        public int? status { get; set; } 
    }
}
