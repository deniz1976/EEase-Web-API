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
            if (request == null || request.username == null || request.targetUsername == null)
                throw new ArgumentNullException(nameof(request));

            var (userInfo, visibilityStatus) = await _profileService.GetUserInfoByNameAsync(request.username, request.targetUsername);

            var response = new GetUserInfoByNameQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserInfoRetrievedSuccessfully),
                Body = new()
                {
                    visibilityStatus = visibilityStatus
                }
            };

            response.Body.Id = userInfo.Id;

            if (visibilityStatus == ProfileVisibilityStatus.BlockedByTarget)
            {
                response.Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedByTarget);
                response.Body.errorMessage = AppMessages.BlockedByTargetProfile;
                return response;
            }
            else if (visibilityStatus == ProfileVisibilityStatus.BlockedTarget)
            {
                response.Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedTarget);
                response.Body.errorMessage = AppMessages.BlockedTargetProfile;
                response.Body.username = userInfo.username;
                return response;
            }

            response.Body.username = userInfo.username;
            response.Body.name = userInfo.name;
            response.Body.surname = userInfo.surname;
            response.Body.bio = userInfo.bio;
            response.Body.photoPath = userInfo.photoPath;
            response.Body.gender = userInfo.gender;
            response.Body.country = userInfo.country;
            response.Body.isFriend = visibilityStatus == ProfileVisibilityStatus.FullAccess &&request.username != request.targetUsername;
            response.Body.canSendFriendRequest = visibilityStatus == ProfileVisibilityStatus.LimitedAccess;
            response.Body.FriendRequestStatus = userInfo.friendRequestStatus;

            if (visibilityStatus == ProfileVisibilityStatus.FullAccess)
            {
                var preferences = await _preferenceService.GetDescriptionsForViewerAsync(request.username, request.targetUsername);
                if (preferences != null)
                {
                    response.Body.PersonalizationPreferences = preferences.PersonalizationPreferences;
                    response.Body.FoodPreferences = preferences.FoodPreferences;
                    response.Body.AccommodationPreferences = preferences.AccommodationPreferences;
                }
            }

            if (visibilityStatus == ProfileVisibilityStatus.LimitedAccess)
            {
                response.Body.errorMessage = AppMessages.NotFriendsProfile;
            }

            return response;
        }
    }
}
