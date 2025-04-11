using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;


namespace EEaseWebAPI.Application.MapEntities.GetUserInfoByName
{
    public class GetUserInfoByName
    {
        public Header? Header { get; set; }
        public GetUserInfoByNameBody? Body { get; set; }
    }

    public class GetUserInfoByNameBody
    {
        public string? username { get; set; }
        public string? name { get; set; }
        public string? surname { get; set; }
        public string? bio { get; set; }
        public string? photoPath { get; set; }
        public string? errorMessage { get; set; }
        public bool canSendFriendRequest { get; set; }
        public bool isFriend { get; set; }

        public string? Id { get; set; }

        public string? gender { get; set; }

        public string? country { get; set; }

        public FriendRequestStatus? FriendRequestStatus { get; set; }


        public EEaseWebAPI.Application.Enums.ProfileVisibilityStatus visibilityStatus { get; set; }
        public List<PreferenceDetail>? PersonalizationPreferences { get; set; }
        public List<PreferenceDetail>? FoodPreferences { get; set; }
        public List<PreferenceDetail>? AccommodationPreferences { get; set; }
    }
} 