using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfo;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Application.Features.Queries.AppUser.StatusCheck;
using EEaseWebAPI.Application.MapEntities.StatusCheck;
using EEaseWebAPI.Application.MapEntities.PreferenceGroups;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Application.DTOs.Route.DislikePlaceOrRestaurantDTO;
using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserService
    {
        Task<CreateUserResponse> CreateAsync(CreateUser model);

        Task<bool> CheckEmailConfirmed(string emailOrUsername);

        Task<bool> EmailConfirm(string code, string usernameOrEmail);

        Task<GetUserInfo> GetUserInfoQuery(string username);

        Task UpdateRefreshTokenAsync(string refreshToken, AppUser user, DateTime accessTokenDate, int addOnAccessTokenDate);

        Task<bool> UpdateUser(UpdateUserCommandRequest request);

        Task<bool> DeleteUserSendMail(string username);

        Task<string> DeleteUserWithCode(string username, string code);

        Task<StatusCheckBody> StatusCheck(string username);

        Task<bool> UpdateUserPreferences(string username, string message);

        Task<bool> ResetUserPreferences(string username);

        Task<bool> UpdateUserCountry(string username, string country);

        Task<UserFriendship> GetFriendshipAsync(string requesterUsername, string AddresseUsername);

        Task<bool> CreateFriendshipAsync(UserFriendship friendship);

        Task<bool> CreateFriendshipAsync(string requesterUsarnem, string AddresseUsername);

        Task<bool> UpdateFriendshipStatusAsync(string requesterId, string addresseeId, FriendshipStatus newStatus);

        Task<IEnumerable<UserFriendship>> GetUserFriendsAsync(string userId);

        Task<IEnumerable<UserFriendship>> GetPendingFriendRequestsAsync(string userId);

        Task<bool> RemoveFriendshipAsync(UserFriendship friendship);

        Task<IEnumerable<UserFriendship>> GetBlockedUsersAsync(string username);

        Task<AppUser> GetUserWithPreferencesAsync(string username);

        Task<GetUserPreferenceDescriptionsBody> GetUserPreferenceDescriptionsAsync(string username);

        GetAllTopicsQueryResponseBody GetAllTopics();

        Task<bool> UpdateUserPreferencesWithTopics(string username, List<string> topics);

        Task<bool> UnblockUserAsync(string targetUsername, string username);

        Task<bool> BlockUserAsync(string targerUsername, string username);

        Task<bool> IsFriendAsync(string username1, string username2);

        Task<bool> SendVerificationEmailAgain(string email);

        Task<bool> CancelFriendRequest(string username, string targetUsername);

        Task<FriendRequestStatus> CheckFriendRequest(string username, string targetUsername);

        Task<bool> UpdateUserCurrency(string username, string currencyCode);

        Task<string> GetUserCurrencyAsync(string username);

        Task<string> GetUserPhotoAsync(string username);

        Task<bool> IsBlockedByUserAsync(string username, string targetUsername);
        
        Task<bool> HasBlockedUserAsync(string username, string targetUsername);
        
        Task<ProfileVisibilityStatus> GetProfileVisibilityStatusAsync(string username, string targetUsername);
        
        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByNameAsync(string username, string targetUsername);
        
        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByIdAsync(string username, string targetUserId);
        
        Task<GetUserPreferenceDescriptionsBody?> GetUserPreferenceDescriptionsByUsernameAsync(string viewerUsername, string targetUsername);

        Task<bool> SetUserPhoto(string username,string photoPath);

    }
}
