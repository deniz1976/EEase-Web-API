using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities.UserProfile;
using EEaseWebAPI.Application.Resources;

namespace EEaseWebAPI.Application.Features.Queries.AppUser
{
    internal static class UserProfileView
    {
        // Looking somebody up by id and by name answer with the same profile, and used to
        // build it in two places: the one that goes by id left out the id, the gender, the
        // country and where the two of them stand with each other.
        public static async Task<(int HeaderCode, UserProfileBody Body)> BuildAsync(
            DTOs.User.GetUserInfo userInfo,
            ProfileVisibilityStatus visibility,
            string viewerUsername,
            IUserPreferenceService preferenceService,
            CancellationToken cancellationToken)
        {
            var body = new UserProfileBody
            {
                Id = userInfo.Id,
                VisibilityStatus = visibility
            };

            if (visibility == ProfileVisibilityStatus.BlockedByTarget)
            {
                body.ErrorMessage = AppMessages.BlockedByTargetProfile;
                return ((int)StatusEnum.UserBlockedByTarget, body);
            }

            if (visibility == ProfileVisibilityStatus.BlockedTarget)
            {
                body.ErrorMessage = AppMessages.BlockedTargetProfile;
                body.Username = userInfo.Username;
                return ((int)StatusEnum.UserBlockedTarget, body);
            }

            body.Username = userInfo.Username;
            body.Name = userInfo.Name;
            body.Surname = userInfo.Surname;
            body.Bio = userInfo.Bio;
            body.PhotoPath = userInfo.PhotoPath;
            body.Gender = userInfo.Gender;
            body.Country = userInfo.Country;
            body.FriendRequestStatus = userInfo.FriendRequestStatus;
            body.IsFriend = visibility == ProfileVisibilityStatus.FullAccess
                            && userInfo.Username != viewerUsername;
            body.CanSendFriendRequest = visibility == ProfileVisibilityStatus.LimitedAccess;

            if (visibility == ProfileVisibilityStatus.FullAccess)
            {
                var preferences = await preferenceService.GetDescriptionsForViewerAsync(
                    viewerUsername, userInfo.Username!, cancellationToken);

                if (preferences is not null)
                {
                    body.PersonalizationPreferences = preferences.PersonalizationPreferences;
                    body.FoodPreferences = preferences.FoodPreferences;
                    body.AccommodationPreferences = preferences.AccommodationPreferences;
                }
            }

            if (visibility == ProfileVisibilityStatus.LimitedAccess)
            {
                body.ErrorMessage = AppMessages.NotFriendsProfile;
            }

            return ((int)StatusEnum.UserInfoRetrievedSuccessfully, body);
        }
    }
}
