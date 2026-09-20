using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoById
{
    public class GetUserInfoByIdQueryHandler : IRequestHandler<GetUserInfoByIdQueryRequest, GetUserInfoByIdQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;
        private readonly IUserPreferenceService _preferenceService;

        public GetUserInfoByIdQueryHandler(
            IHeaderService headerService,
            IUserProfileService profileService,
            IUserPreferenceService preferenceService)
        {
            _headerService = headerService;
            _profileService = profileService;
            _preferenceService = preferenceService;
        }

        public async Task<GetUserInfoByIdQueryResponse> Handle(GetUserInfoByIdQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.username == null || request.userId == null)
                throw new ArgumentNullException(nameof(request));

            var (userInfo, visibilityStatus) = await _profileService.GetUserInfoByIdAsync(request.username, request.userId);

            var response = new GetUserInfoByIdQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserInfoRetrievedSuccessfully),
                Body = new()
                {
                    visibilityStatus = visibilityStatus
                }
            };

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
            response.Body.isFriend = visibilityStatus == ProfileVisibilityStatus.FullAccess && userInfo.username != request.username;
            response.Body.canSendFriendRequest = visibilityStatus == ProfileVisibilityStatus.LimitedAccess;

            if (visibilityStatus == ProfileVisibilityStatus.FullAccess)
            {
                var preferences = await _preferenceService.GetDescriptionsForViewerAsync(request.username, userInfo.username);
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
