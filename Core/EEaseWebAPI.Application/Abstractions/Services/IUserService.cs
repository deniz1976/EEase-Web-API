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
    /// <summary>
    /// Manages user-related operations including creation, authentication, profile management, and preferences.
    /// Provides comprehensive user management functionality for the application.
    /// </summary>
    public interface IUserService
    {
        /// <summary>
        /// Creates a new user account with the specified details.
        /// </summary>
        /// <param name="model">User creation model containing required user information</param>
        /// <returns>Response containing the created user's details and status</returns>
        /// <remarks>
        /// - Validates user input
        /// - Checks for existing users
        /// - Handles password hashing
        /// - Generates confirmation tokens
        /// </remarks>
        Task<CreateUserResponse> CreateAsync(CreateUser model);

        /// <summary>
        /// Verifies if a user's email has been confirmed.
        /// </summary>
        /// <param name="emailOrUsername">Email address or username to check</param>
        /// <returns>True if email is confirmed, false otherwise</returns>
        Task<bool> CheckEmailConfirmed(string emailOrUsername);

        /// <summary>
        /// Confirms a user's email using the provided confirmation code.
        /// </summary>
        /// <param name="code">Email confirmation code</param>
        /// <param name="usernameOrEmail">Username or email of the user</param>
        /// <returns>True if confirmation successful, false otherwise</returns>
        Task<bool> EmailConfirm(string code, string usernameOrEmail);

        /// <summary>
        /// Retrieves detailed user information based on email or username.
        /// </summary>
        /// <param name="username">Email or username of the user</param>
        /// <returns>User information including profile details and preferences</returns>
        Task<GetUserInfo> GetUserInfoQuery(string username);

        /// <summary>
        /// Updates the refresh token for a user's authentication session.
        /// </summary>
        /// <param name="refreshToken">New refresh token to be set</param>
        /// <param name="user">User whose token needs to be updated</param>
        /// <param name="accessTokenDate">Date when the access token was issued</param>
        /// <param name="addOnAccessTokenDate">Additional time to add to token expiration</param>
        /// <remarks>
        /// Handles token rotation and expiration management for security
        /// </remarks>
        Task UpdateRefreshTokenAsync(string refreshToken, AppUser user, DateTime accessTokenDate, int addOnAccessTokenDate);

        /// <summary>
        /// Updates a user's profile information.
        /// </summary>
        /// <param name="request">Request containing updated user information</param>
        /// <returns>True if update successful, false otherwise</returns>
        /// <remarks>
        /// - Validates updated information
        /// - Updates profile details
        /// - Maintains data consistency
        /// </remarks>
        Task<bool> UpdateUser(UpdateUserCommandRequest request);

        /// <summary>
        /// Initiates the account deletion process by sending a confirmation email.
        /// </summary>
        /// <param name="username">Username of the account to delete</param>
        /// <returns>True if deletion email sent successfully, false otherwise</returns>
        Task<bool> DeleteUserSendMail(string username);

        /// <summary>
        /// Completes the account deletion process using a confirmation code.
        /// </summary>
        /// <param name="username">Username of the account to delete</param>
        /// <param name="code">Confirmation code from the deletion email</param>
        /// <returns>Success message if deletion completed</returns>
        Task<string> DeleteUserWithCode(string username, string code);

        /// <summary>
        /// Checks the current status of a user's account.
        /// </summary>
        /// <param name="username">Username to check status for</param>
        /// <returns>Current account status information</returns>
        Task<StatusCheckBody> StatusCheck(string username);

        /// <summary>
        /// Updates user preferences based on a natural language message.
        /// </summary>
        /// <param name="username">Username whose preferences to update</param>
        /// <param name="message">Natural language message containing preference information</param>
        /// <returns>True if preferences updated successfully, false otherwise</returns>
        Task<bool> UpdateUserPreferences(string username, string message);

        /// <summary>
        /// Resets all user preferences to default values.
        /// </summary>
        /// <param name="username">Username whose preferences to reset</param>
        /// <returns>True if preferences reset successfully, false otherwise</returns>
        Task<bool> ResetUserPreferences(string username);

        /// <summary>
        /// Updates a user's country information.
        /// </summary>
        /// <param name="username">Username whose country to update</param>
        /// <param name="country">New country value</param>
        /// <returns>True if country updated successfully, false otherwise</returns>
        Task<bool> UpdateUserCountry(string username, string country);

        /// <summary>
        /// Retrieves friendship information between two users.
        /// </summary>
        /// <param name="requesterUsername">Username of the requesting user</param>
        /// <param name="AddresseUsername">Username of the target user</param>
        /// <returns>Friendship details if exists</returns>
        Task<UserFriendship> GetFriendshipAsync(string requesterUsername, string AddresseUsername);

        /// <summary>
        /// Creates a new friendship relationship between users.
        /// </summary>
        /// <param name="friendship">Friendship object containing relationship details</param>
        /// <returns>True if friendship created successfully, false otherwise</returns>
        Task<bool> CreateFriendshipAsync(UserFriendship friendship);

        /// <summary>
        /// Creates a new friendship relationship using usernames.
        /// </summary>
        /// <param name="requesterUsarnem">Username of the requesting user</param>
        /// <param name="AddresseUsername">Username of the target user</param>
        /// <returns>True if friendship created successfully, false otherwise</returns>
        Task<bool> CreateFriendshipAsync(string requesterUsarnem, string AddresseUsername);

        /// <summary>
        /// Updates the status of a friendship relationship.
        /// </summary>
        /// <param name="requesterId">ID of the requesting user</param>
        /// <param name="addresseeId">ID of the target user</param>
        /// <param name="newStatus">New friendship status to set</param>
        /// <returns>True if status updated successfully, false otherwise</returns>
        Task<bool> UpdateFriendshipStatusAsync(string requesterId, string addresseeId, FriendshipStatus newStatus);

        /// <summary>
        /// Retrieves all friendships for a specific user.
        /// </summary>
        /// <param name="userId">User ID to get friends for</param>
        /// <returns>Collection of user's friendships</returns>
        Task<IEnumerable<UserFriendship>> GetUserFriendsAsync(string userId);

        /// <summary>
        /// Retrieves pending friend requests for a user.
        /// </summary>
        /// <param name="userId">User ID to get pending requests for</param>
        /// <returns>Collection of pending friendship requests</returns>
        Task<IEnumerable<UserFriendship>> GetPendingFriendRequestsAsync(string userId);

        /// <summary>
        /// Removes a friendship relationship between users.
        /// </summary>
        /// <param name="friendship">Friendship object to remove</param>
        /// <returns>True if friendship removed successfully, false otherwise</returns>
        Task<bool> RemoveFriendshipAsync(UserFriendship friendship);

        /// <summary>
        /// Retrieves list of users blocked by a specific user.
        /// </summary>
        /// <param name="username">Username to get blocked users for</param>
        /// <returns>Collection of blocked user relationships</returns>
        Task<IEnumerable<UserFriendship>> GetBlockedUsersAsync(string username);

        /// <summary>
        /// Retrieves user information including their preferences.
        /// </summary>
        /// <param name="username">Username to get information for</param>
        /// <returns>User object with preferences included</returns>
        Task<AppUser> GetUserWithPreferencesAsync(string username);

        /// <summary>
        /// Retrieves descriptions of a user's preferences.
        /// </summary>
        /// <param name="username">Username to get preference descriptions for</param>
        /// <returns>Formatted descriptions of user preferences</returns>
        Task<GetUserPreferenceDescriptionsBody> GetUserPreferenceDescriptionsAsync(string username);

        /// <summary>
        /// Retrieves all available preference topics.
        /// </summary>
        /// <returns>Response containing all available topics</returns>
        GetAllTopicsQueryResponseBody GetAllTopics();

        /// <summary>
        /// Updates user preferences based on selected topics.
        /// </summary>
        /// <param name="username">Username whose preferences to update</param>
        /// <param name="topics">List of selected topic identifiers</param>
        /// <returns>True if preferences updated successfully, false otherwise</returns>
        Task<bool> UpdateUserPreferencesWithTopics(string username, List<string> topics);

        /// <summary>
        /// Removes a user from the blocked list.
        /// </summary>
        /// <param name="targetUsername">Username to unblock</param>
        /// <param name="username">Username performing the unblock</param>
        /// <returns>True if user unblocked successfully, false otherwise</returns>
        Task<bool> UnblockUserAsync(string targetUsername, string username);

        /// <summary>
        /// Adds a user to the blocked list.
        /// </summary>
        /// <param name="targerUsername">Username to block</param>
        /// <param name="username">Username performing the block</param>
        /// <returns>True if user blocked successfully, false otherwise</returns>
        Task<bool> BlockUserAsync(string targerUsername, string username);

        /// <summary>
        /// Checks if two users are friends.
        /// </summary>
        /// <param name="username1">First user's username</param>
        /// <param name="username2">Second user's username</param>
        /// <returns>True if users are friends, false otherwise</returns>
        Task<bool> IsFriendAsync(string username1, string username2);

        /// <summary>
        /// Resends the email verification code to a user's email address.
        /// </summary>
        /// <param name="email">Email address to send the verification code to</param>
        /// <returns>True if verification email sent successfully, false otherwise</returns>
        Task<bool> SendVerificationEmailAgain(string email);

        /// <summary>
        /// Cancels a pending friend request sent by the user.
        /// </summary>
        /// <param name="username">Username of the user who sent the request</param>
        /// <param name="targetUsername">Username of the request recipient</param>
        /// <returns>True if request canceled successfully, false otherwise</returns>
        Task<bool> CancelFriendRequest(string username, string targetUsername);

        /// <summary>
        /// Checks the status of a friend request between two users.
        /// </summary>
        /// <param name="username">Username of the requesting user</param>
        /// <param name="targetUsername">Username of the target user</param>
        /// <returns>Status of the friend request (NoRequest, Requester, Addressee, AlreadyFriends, Blocked)</returns>
        Task<FriendRequestStatus> CheckFriendRequest(string username, string targetUsername);

        /// <summary>
        /// Updates a user's preferred currency.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="currencyCode">Three-letter currency code (e.g., USD, EUR)</param>
        /// <returns>True if currency updated successfully, false otherwise</returns>
        Task<bool> UpdateUserCurrency(string username, string currencyCode);

        /// <summary>
        /// Retrieves a user's preferred currency.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <returns>Three-letter currency code of the user's preferred currency</returns>
        Task<string> GetUserCurrencyAsync(string username);

        /// <summary>
        /// Retrieves the path to a user's profile photo.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <returns>Path to the user's profile photo</returns>
        Task<string> GetUserPhotoAsync(string username);

        /// <summary>
        /// Checks if a user is blocked by another user.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="targetUsername">Username of the target user</param>
        /// <returns>True if user is blocked by target, false otherwise</returns>
        Task<bool> IsBlockedByUserAsync(string username, string targetUsername);
        
        /// <summary>
        /// Checks if a user has blocked another user.
        /// </summary>
        /// <param name="username">Username of the user</param>
        /// <param name="targetUsername">Username of the target user</param>
        /// <returns>True if user has blocked target, false otherwise</returns>
        Task<bool> HasBlockedUserAsync(string username, string targetUsername);
        
        /// <summary>
        /// Gets the profile visibility status between two users.
        /// </summary>
        /// <param name="username">Username of the viewing user</param>
        /// <param name="targetUsername">Username of the profile owner</param>
        /// <returns>ProfileVisibilityStatus indicating the level of access</returns>
        Task<ProfileVisibilityStatus> GetProfileVisibilityStatusAsync(string username, string targetUsername);
        
        /// <summary>
        /// Gets limited user information by username for profile viewing.
        /// </summary>
        /// <param name="username">Username of the viewing user</param>
        /// <param name="targetUsername">Username of the profile owner</param>
        /// <returns>User information with appropriate visibility based on relationship</returns>
        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByNameAsync(string username, string targetUsername);
        
        /// <summary>
        /// Gets limited user information by ID for profile viewing.
        /// </summary>
        /// <param name="username">Username of the viewing user</param>
        /// <param name="targetUserId">User ID of the profile owner</param>
        /// <returns>User information with appropriate visibility based on relationship</returns>
        Task<(GetUserInfo userInfo, ProfileVisibilityStatus visibilityStatus)> GetUserInfoByIdAsync(string username, string targetUserId);
        
        /// <summary>
        /// Retrieves preference descriptions for a specific user by username.
        /// </summary>
        /// <param name="viewerUsername">Username of the viewing user</param>
        /// <param name="targetUsername">Username of the user whose preferences to retrieve</param>
        /// <returns>User preference descriptions if allowed to view, null otherwise</returns>
        Task<GetUserPreferenceDescriptionsBody?> GetUserPreferenceDescriptionsByUsernameAsync(string viewerUsername, string targetUsername);

        Task<bool> SetUserPhoto(string username,string photoPath);

        
    }
}
