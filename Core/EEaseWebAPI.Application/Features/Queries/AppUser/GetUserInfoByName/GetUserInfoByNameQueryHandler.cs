using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
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
        private readonly IUserService _userService;

        public GetUserInfoByNameQueryHandler(IHeaderService headerService, IUserService userService)
        {
            _headerService = headerService;
            _userService = userService;
        }

        public async Task<GetUserInfoByNameQueryResponse> Handle(GetUserInfoByNameQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.username == null || request.targetUsername == null)
                throw new ArgumentNullException(nameof(request));

            var (userInfo, visibilityStatus) = await _userService.GetUserInfoByNameAsync(request.username, request.targetUsername);
            
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
                response.response.Body.errorMessage = "This user has blocked you. You cannot view their profile.";
                return response;
            }
            else if (visibilityStatus == ProfileVisibilityStatus.BlockedTarget)
            {
                response.response.Header = _headerService.HeaderCreate((int)StatusEnum.UserBlockedTarget);
                response.response.Body.errorMessage = "You have blocked this user. Unblock them to view their profile.";
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
                var preferences = await _userService.GetUserPreferenceDescriptionsByUsernameAsync(request.username, request.targetUsername);
                if (preferences != null)
                {
                    response.response.Body.PersonalizationPreferences = preferences.PersonalizationPreferences;
                    response.response.Body.FoodPreferences = preferences.FoodPreferences;
                    response.response.Body.AccommodationPreferences = preferences.AccommodationPreferences;
                }
            }
            
            if (visibilityStatus == ProfileVisibilityStatus.LimitedAccess)
            {
                response.response.Body.errorMessage = "You are not friends with this user. Send a friend request to see more details.";
            }

            return response;
        }
    }
} 