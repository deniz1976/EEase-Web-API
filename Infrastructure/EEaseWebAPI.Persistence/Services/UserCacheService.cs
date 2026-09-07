using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Services
{
    public class UserCacheService : IUserCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly EEaseAPIDbContext _context;
        private const string USER_CACHE_KEY = "ALL_USERS";

        public UserCacheService(IMemoryCache memoryCache, EEaseAPIDbContext context)
        {
            _memoryCache = memoryCache;
            _context = context;
        }

        public async Task LoadUsersToCache()
        {
            if (!_memoryCache.TryGetValue(USER_CACHE_KEY, out _))
            {
                var users = await _context.Users
                    .AsNoTracking()
                    .Where(u => u.EmailConfirmed)
                    .Select(u => new UserSearchDTO
                    {
                        Id = u.Id,
                        Username = u.UserName ?? string.Empty,
                        Name = u.Name ?? string.Empty,
                        Surname = u.Surname ?? string.Empty,
                        PhotoUrl = u.PhotoPath ?? string.Empty,
                        Gender = u.Gender ?? string.Empty
                    })
                    .ToListAsync();

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(USER_CACHE_KEY, users, cacheEntryOptions);
            }
        }

        public List<UserSearchDTO> SearchUsers(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return new List<UserSearchDTO>();

            if (_memoryCache.TryGetValue(USER_CACHE_KEY, out List<UserSearchDTO> users))
            {
                searchTerm = searchTerm.ToLower().Trim();
                return users
                    .Where(u =>
                        (u.Username?.ToLower().Contains(searchTerm) ?? false) ||
                        (u.Name?.ToLower().Contains(searchTerm) ?? false) ||
                        (u.Surname?.ToLower().Contains(searchTerm) ?? false) ||
                        ((u.Name + " " + u.Surname)?.ToLower().Contains(searchTerm) ?? false))
                    .Take(10)
                    .ToList();
            }

            return new List<UserSearchDTO>();
        }

        public void AddOrUpdateUserInCache(AppUser user)
        {
            if (!user.EmailConfirmed) return;

            if (_memoryCache.TryGetValue(USER_CACHE_KEY, out List<UserSearchDTO> users))
            {
                var existingUser = users.FirstOrDefault(u => u.Id == user.Id);
                var updatedUser = new UserSearchDTO
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Name = user.Name ?? string.Empty,
                    Surname = user.Surname ?? string.Empty,
                    PhotoUrl = user.PhotoPath ?? string.Empty,
                    Gender = user.Gender ?? string.Empty
                };

                if (existingUser != null)
                {
                    var index = users.IndexOf(existingUser);
                    users[index] = updatedUser;
                }
                else
                {
                    users.Add(updatedUser);
                }

                var cacheEntryOptions = new MemoryCacheEntryOptions()
                    .SetSlidingExpiration(TimeSpan.FromHours(1));

                _memoryCache.Set(USER_CACHE_KEY, users, cacheEntryOptions);
            }
        }

        public void UpdateUserAttributesInCache(string userId, string? username = null, string? name = null, string? surname = null, string? photoUrl = null, string? gender = null)
        {
            if (_memoryCache.TryGetValue(USER_CACHE_KEY, out List<UserSearchDTO> users))
            {
                var existingUser = users.FirstOrDefault(u => u.Id == userId);

                if (existingUser != null)
                {
                    if (username != null)
                        existingUser.Username = username;

                    if (name != null)
                        existingUser.Name = name;

                    if (surname != null)
                        existingUser.Surname = surname;

                    if (photoUrl != null)
                        existingUser.PhotoUrl = photoUrl;

                    if (gender != null)
                        existingUser.Gender = gender;

                    var cacheEntryOptions = new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(1));

                    _memoryCache.Set(USER_CACHE_KEY, users, cacheEntryOptions);
                }
            }
        }

        public void RemoveUserFromCache(string userId)
        {
            if (_memoryCache.TryGetValue(USER_CACHE_KEY, out List<UserSearchDTO> users))
            {
                var userToRemove = users.FirstOrDefault(u => u.Id == userId);
                if (userToRemove != null)
                {
                    users.Remove(userToRemove);
                    _memoryCache.Set(USER_CACHE_KEY, users, new MemoryCacheEntryOptions()
                        .SetSlidingExpiration(TimeSpan.FromHours(1)));
                }
            }
        }
    }
}
