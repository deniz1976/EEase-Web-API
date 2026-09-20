using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.GetUserInfoById
{

    public class GetUserInfoByIdBody
    {
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public string? Bio { get; set; }
        public string? PhotoPath { get; set; }
        public string? ErrorMessage { get; set; }
        public bool CanSendFriendRequest { get; set; }
        public bool IsFriend { get; set; }
        public EEaseWebAPI.Application.Enums.ProfileVisibilityStatus VisibilityStatus { get; set; }
        public List<PreferenceDetail>? PersonalizationPreferences { get; set; }
        public List<PreferenceDetail>? FoodPreferences { get; set; }
        public List<PreferenceDetail>? AccommodationPreferences { get; set; }
    }
}
