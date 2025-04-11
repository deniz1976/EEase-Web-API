using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Identity
{
    public class AppUser : IdentityUser<string>
    {
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Gender { get; set; }
        public DateOnly? BornDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public string? DeleteCode { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenEndDate { get; set; }
        public string? VerificationCode { get; set; }
        public string? ResetPasswordCode { get; set; }
        public bool? status { get; set; } = true;
        public string? Currency { get; set; } = "TRY";
        public string? Country { get; set; } = "Turkey";
        public string? Bio { get; set; }
        public string? PhotoPath { get; set; }
        public DateTime? LastSeen { get; set; } = null;
        public UserAccommodationPreferences? AccommodationPreferences { get; set; } = null;
        public UserFoodPreferences? FoodPreferences { get; set; } = null;
        public UserPersonalization? UserPersonalization { get; set; } = null;
        public List<StandardRoute>? LikedRoutes { get; set; }
        public List<StandardRoute>? MyRoutes { get; set; }
        public virtual ICollection<UserFriendship> SentFriendRequests { get; set; }
        public virtual ICollection<UserFriendship> ReceivedFriendRequests { get; set; }
        public AppUser()
        {
            SentFriendRequests = new HashSet<UserFriendship>();
            ReceivedFriendRequests = new HashSet<UserFriendship>();
            LikedRoutes = new List<StandardRoute>(){};
            MyRoutes = new List<StandardRoute>(){};
        }
    }
}
