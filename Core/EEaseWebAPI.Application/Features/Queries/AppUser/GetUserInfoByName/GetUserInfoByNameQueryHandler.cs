using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoByName
{
    public class GetUserInfoByNameQueryHandler : IRequestHandler<GetUserInfoByNameQueryRequest, GetUserInfoByNameQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;
        private readonly IUserPreferenceService _preferenceService;

        public GetUserInfoByNameQueryHandler(
            IHeaderService headerService,
            IUserProfileService profileService,
            IUserPreferenceService preferenceService)
        {
            _headerService = headerService;
            _profileService = profileService;
            _preferenceService = preferenceService;
        }

        public async Task<GetUserInfoByNameQueryResponse> Handle(GetUserInfoByNameQueryRequest request, CancellationToken cancellationToken)
        {
var (userInfo, visibilityStatus) = await _profileService.GetUserInfoByNameAsync(request.Username, request.TargetUsername);

            var response = new GetUserInfoByNameQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserInfoRetrievedSuccessfully),
                Body = new()
                {
                    VisibilityStatus = visibilityStatus
                }
            };

            response.Body.Id = userInfo.Id;

            if (visibilityStatus == ProfileVisibilityStatus.BlockedByTarget)
            {
                response.Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedByTarget);
                response.Body.ErrorMessage = AppMessages.BlockedByTargetProfile;
                return response;
            }
            else if (visibilityStatus == ProfileVisibilityStatus.BlockedTarget)
            {
                response.Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedTarget);
                response.Body.ErrorMessage = AppMessages.BlockedTargetProfile;
                response.Body.Username = userInfo.Username;
                return response;
            }

            response.Body.Username = userInfo.Username;
            response.Body.Name = userInfo.Name;
            response.Body.Surname = userInfo.Surname;
            response.Body.Bio = userInfo.Bio;
            response.Body.PhotoPath = userInfo.PhotoPath;
            response.Body.Gender = userInfo.Gender;
            response.Body.Country = userInfo.Country;
            response.Body.IsFriend = visibilityStatus == ProfileVisibilityStatus.FullAccess &&request.Username != request.TargetUsername;
            response.Body.CanSendFriendRequest = visibilityStatus == ProfileVisibilityStatus.LimitedAccess;
            response.Body.FriendRequestStatus = userInfo.FriendRequestStatus;

            if (visibilityStatus == ProfileVisibilityStatus.FullAccess)
            {
                var preferences = await _preferenceService.GetDescriptionsForViewerAsync(request.Username, request.TargetUsername);
                if (preferences != null)
                {
                    response.Body.PersonalizationPreferences = preferences.PersonalizationPreferences;
                    response.Body.FoodPreferences = preferences.FoodPreferences;
                    response.Body.AccommodationPreferences = preferences.AccommodationPreferences;
                }
            }

            if (visibilityStatus == ProfileVisibilityStatus.LimitedAccess)
            {
                response.Body.ErrorMessage = AppMessages.NotFriendsProfile;
            }

            return response;
        }
    }
}
