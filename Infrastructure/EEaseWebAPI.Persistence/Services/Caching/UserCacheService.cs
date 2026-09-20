using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence.Services.Caching
{
    /// <summary>
    /// Keeps the confirmed users in memory so that search does not hit the database on every
    /// keystroke. The cached list is never edited in place: a change publishes a new list, so
    /// a request reading it cannot see a half finished update.
    /// </summary>
    public sealed class UserCacheService : IUserCacheService
    {
        private const int MaxResults = 10;

        private static readonly object WriteLock = new();

        private readonly IMemoryCache _memoryCache;
        private readonly EEaseAPIDbContext _context;
        private readonly CacheOptions _options;

        public UserCacheService(
            IMemoryCache memoryCache,
            EEaseAPIDbContext context,
            IOptions<CacheOptions> options)
        {
            _memoryCache = memoryCache;
            _context = context;
            _options = options.Value;
        }

        public async Task LoadUsersToCache()
        {
            if (Cached() is null)
            {
                Publish(await ReadUsersAsync());
            }
        }

        public async Task<List<UserSearchDTO>> SearchUsersAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
            {
                return new List<UserSearchDTO>();
            }

            var users = Cached();

            if (users is null)
            {
                users = await ReadUsersAsync();
                Publish(users);
            }

            var term = searchTerm.Trim();

            return users
                .Where(user => Matches(user, term))
                .Take(MaxResults)
                .ToList();
        }

        public void AddOrUpdateUserInCache(AppUser user)
        {
            if (!user.EmailConfirmed)
            {
                return;
            }

            Replace(user.Id, _ => ToSearchResult(user), addWhenMissing: true);
        }

        public void UpdateUserAttributesInCache(
            string userId,
            string? username = null,
            string? name = null,
            string? surname = null,
            string? photoUrl = null,
            string? gender = null)
        {
            Replace(userId, existing => new UserSearchDTO
            {
                Id = existing.Id,
                Username = username ?? existing.Username,
                Name = name ?? existing.Name,
                Surname = surname ?? existing.Surname,
                PhotoUrl = photoUrl ?? existing.PhotoUrl,
                Gender = gender ?? existing.Gender
            });
        }

        public void RemoveUserFromCache(string userId)
        {
            lock (WriteLock)
            {
                var users = Cached();

                if (users is null)
                {
                    return;
                }

                Publish(users.Where(user => user.Id != userId).ToList());
            }
        }

        private void Replace(string userId, Func<UserSearchDTO, UserSearchDTO> update, bool addWhenMissing = false)
        {
            lock (WriteLock)
            {
                var users = Cached();

                if (users is null)
                {
                    return;
                }

                var existing = users.FirstOrDefault(user => user.Id == userId);

                if (existing is null && !addWhenMissing)
                {
                    return;
                }

                var updated = users.Where(user => user.Id != userId).ToList();

                updated.Add(update(existing ?? new UserSearchDTO { Id = userId }));

                Publish(updated);
            }
        }

        private List<UserSearchDTO>? Cached() =>
            _memoryCache.TryGetValue(_options.UsersCacheKey, out List<UserSearchDTO>? users) ? users : null;

        private void Publish(List<UserSearchDTO> users) =>
            _memoryCache.Set(_options.UsersCacheKey, users, new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromHours(_options.UserLifetimeHours)));

        private Task<List<UserSearchDTO>> ReadUsersAsync() =>
            _context.Users
                .AsNoTracking()
                .Where(user => user.EmailConfirmed)
                .Select(user => new UserSearchDTO
                {
                    Id = user.Id,
                    Username = user.UserName ?? string.Empty,
                    Name = user.Name ?? string.Empty,
                    Surname = user.Surname ?? string.Empty,
                    PhotoUrl = user.PhotoPath ?? string.Empty,
                    Gender = user.Gender ?? string.Empty
                })
                .ToListAsync();

        private static UserSearchDTO ToSearchResult(AppUser user) => new()
        {
            Id = user.Id,
            Username = user.UserName ?? string.Empty,
            Name = user.Name ?? string.Empty,
            Surname = user.Surname ?? string.Empty,
            PhotoUrl = user.PhotoPath ?? string.Empty,
            Gender = user.Gender ?? string.Empty
        };

        // Ordinal on purpose: a culture aware comparison folds the Turkish dotted and dotless
        // i differently on the server than the caller typed them.
        private static bool Matches(UserSearchDTO user, string term) =>
            Contains(user.Username, term) ||
            Contains(user.Name, term) ||
            Contains(user.Surname, term) ||
            Contains($"{user.Name} {user.Surname}", term);

        private static bool Contains(string? value, string term) =>
            value?.Contains(term, StringComparison.OrdinalIgnoreCase) == true;
    }
}
