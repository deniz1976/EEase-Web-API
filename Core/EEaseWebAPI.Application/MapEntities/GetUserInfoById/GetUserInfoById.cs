using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.MapEntities.GetUserInfoById
{
    public class GetUserInfoById
    {
        public Header? Header { get; set; }
        public GetUserInfoByIdBody? Body { get; set; }
    }

    public class GetUserInfoByIdBody
    {
        public string? username { get; set; }
        public string? name { get; set; }
        public string? surname { get; set; }
        public string? bio { get; set; }
        public string? photoPath { get; set; }
        public string? errorMessage { get; set; }
        public bool canSendFriendRequest { get; set; }
        public bool isFriend { get; set; }
        public EEaseWebAPI.Application.Enums.ProfileVisibilityStatus visibilityStatus { get; set; }
        public List<PreferenceDetail>? PersonalizationPreferences { get; set; }
        public List<PreferenceDetail>? FoodPreferences { get; set; }
        public List<PreferenceDetail>? AccommodationPreferences { get; set; }
    }
} 