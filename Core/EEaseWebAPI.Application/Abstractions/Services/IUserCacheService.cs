using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Service interface for managing user data caching operations.
    /// Provides methods for loading, searching, updating, and removing user data from cache.
    /// </summary>
    public interface IUserCacheService
    {
        /// <summary>
        /// Loads all confirmed users into the cache.
        /// Only loads if cache is empty to prevent unnecessary database queries.
        /// </summary>
        Task LoadUsersToCache();

        /// <summary>
        /// Searches for users in the cache based on a search term.
        /// Searches through username, name, and surname fields.
        /// </summary>
        /// <param name="searchTerm">The term to search for in user data</param>
        /// <returns>A list of matching users, limited to 10 results</returns>
        List<UserSearchDTO> SearchUsers(string searchTerm);

        /// <summary>
        /// Adds a new user to the cache or updates an existing user's information.
        /// Only processes users with confirmed emails.
        /// </summary>
        /// <param name="user">The user to add or update in cache</param>
        void AddOrUpdateUserInCache(AppUser user);
        
        /// <summary>
        /// Updates specified user attributes in the cache. If any of the provided information is null,
        /// that attribute will not be updated.
        /// </summary>
        /// <param name="userId">User ID</param>
        /// <param name="username">Username to update (not updated if null)</param>
        /// <param name="name">First name to update (not updated if null)</param>
        /// <param name="surname">Last name to update (not updated if null)</param>
        /// <param name="photoUrl">Photo URL to update (not updated if null)</param>
        void UpdateUserAttributesInCache(string userId, string? username = null, string? name = null, string? surname = null, string? photoUrl = null,string? Gender = null);

        /// <summary>
        /// Removes a user from the cache based on their ID.
        /// Used when a user is deleted from the system.
        /// </summary>
        /// <param name="userId">The ID of the user to remove from cache</param>
        void RemoveUserFromCache(string userId);
    }
} 