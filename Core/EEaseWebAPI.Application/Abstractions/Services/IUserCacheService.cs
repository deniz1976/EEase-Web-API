using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IUserCacheService
    {
        Task LoadUsersToCache();

        List<UserSearchDTO> SearchUsers(string searchTerm);

        void AddOrUpdateUserInCache(AppUser user);

        void UpdateUserAttributesInCache(string userId, string? username = null, string? name = null, string? surname = null, string? photoUrl = null,string? Gender = null);

        void RemoveUserFromCache(string userId);
    }
}
