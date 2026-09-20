using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserCacheService
    {
        Task LoadUsersToCache();

        /// <summary>
        /// Loads the list when it is not cached, so a search never comes back empty just
        /// because the entry expired.
        /// </summary>
        Task<List<UserSearchDTO>> SearchUsersAsync(string searchTerm);

        void AddOrUpdateUserInCache(AppUser user);

        void UpdateUserAttributesInCache(
            string userId,
            string? username = null,
            string? name = null,
            string? surname = null,
            string? photoUrl = null,
            string? gender = null);

        void RemoveUserFromCache(string userId);
    }
}
