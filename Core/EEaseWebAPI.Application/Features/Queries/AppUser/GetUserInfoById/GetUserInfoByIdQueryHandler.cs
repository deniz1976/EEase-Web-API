using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
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
        private readonly IUserService _userService;

        public GetUserInfoByIdQueryHandler(IHeaderService headerService, IUserService userService)
        {
            _headerService = headerService;
            _userService = userService;
        }

        public async Task<GetUserInfoByIdQueryResponse> Handle(GetUserInfoByIdQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.username == null || request.userId == null)
                throw new ArgumentNullException(nameof(request));

            var (userInfo, visibilityStatus) = await _userService.GetUserInfoByIdAsync(request.username, request.userId);
            
            var response = new GetUserInfoByIdQueryResponse
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

            // Add appropriate error message based on visibility status
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
                // When user has blocked target, only return username
                response.response.Body.username = userInfo.username;
                return response;
            }
            
            // Only add user information if not blocked
            response.response.Body.username = userInfo.username;
            response.response.Body.name = userInfo.name;
            response.response.Body.surname = userInfo.surname;
            response.response.Body.bio = userInfo.bio;
            response.response.Body.photoPath = userInfo.photoPath;
            response.response.Body.isFriend = visibilityStatus == ProfileVisibilityStatus.FullAccess && userInfo.username != request.username;
            response.response.Body.canSendFriendRequest = visibilityStatus == ProfileVisibilityStatus.LimitedAccess;
            
            // Add user preferences if they are friends or the same user
            if (visibilityStatus == ProfileVisibilityStatus.FullAccess)
            {
                var preferences = await _userService.GetUserPreferenceDescriptionsByUsernameAsync(request.username, userInfo.username);
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