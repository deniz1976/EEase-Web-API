using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoByName
{
    public class GetUserInfoByNameQueryHandler : IRequestHandler<GetUserInfoByNameQueryRequest, GetUserInfoByNameQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;
        private readonly IUserPreferenceService _preferenceService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public GetUserInfoByNameQueryHandler(IHeaderService headerService, IUserProfileService profileService,

            IStringLocalizer<AppMessages> messages)

        {
            _headerService = headerService;
            _profileService = profileService;

            _messages = messages;
        }

        public async Task<GetUserInfoByNameQueryResponse> Handle(GetUserInfoByNameQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.username == null || request.targetUsername == null)
                throw new ArgumentNullException(nameof(request));

            var (userInfo, visibilityStatus) = await _profileService.GetUserInfoByNameAsync(request.username, request.targetUsername);

            var response = new GetUserInfoByNameQueryResponse
            {
                response = new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserInfoRetrievedSuccessfully),
                    Body = new()
                    {
                        visibilityStatus = visibilityStatus
                    }
                }
            };

            response.response.Body.Id = userInfo.Id;

            if (visibilityStatus == ProfileVisibilityStatus.BlockedByTarget)
            {
                response.response.Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedByTarget);
                response.response.Body.errorMessage = _messages["BlockedByTargetProfile"];
                return response;
            }
            else if (visibilityStatus == ProfileVisibilityStatus.BlockedTarget)
            {
                response.response.Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedTarget);
                response.response.Body.errorMessage = _messages["BlockedTargetProfile"];
                response.response.Body.username = userInfo.username;
                return response;
            }

            response.response.Body.username = userInfo.username;
            response.response.Body.name = userInfo.name;
            response.response.Body.surname = userInfo.surname;
            response.response.Body.bio = userInfo.bio;
            response.response.Body.photoPath = userInfo.photoPath;
            response.response.Body.gender = userInfo.gender;
            response.response.Body.country = userInfo.country;
            response.response.Body.isFriend = visibilityStatus == ProfileVisibilityStatus.FullAccess &&request.username != request.targetUsername;
            response.response.Body.canSendFriendRequest = visibilityStatus == ProfileVisibilityStatus.LimitedAccess;
            response.response.Body.FriendRequestStatus = userInfo.friendRequestStatus;

            if (visibilityStatus == ProfileVisibilityStatus.FullAccess)
            {
                var preferences = await _preferenceService.GetDescriptionsForViewerAsync(request.username, request.targetUsername);
                if (preferences != null)
                {
                    response.response.Body.PersonalizationPreferences = preferences.PersonalizationPreferences;
                    response.response.Body.FoodPreferences = preferences.FoodPreferences;
                    response.response.Body.AccommodationPreferences = preferences.AccommodationPreferences;
                }
            }

            if (visibilityStatus == ProfileVisibilityStatus.LimitedAccess)
            {
                response.response.Body.errorMessage = _messages["NotFriendsProfile"];
            }

            return response;
        }
    }
}
