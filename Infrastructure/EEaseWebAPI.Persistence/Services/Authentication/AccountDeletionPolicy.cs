using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace EEaseWebAPI.Persistence.Services.Authentication
{
    public sealed class AccountDeletionPolicy : IAccountDeletionPolicy
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly EEaseAPIDbContext _context;
        private readonly IUserCacheService _userCacheService;

        public AccountDeletionPolicy(
            UserManager<AppUser> userManager,
            EEaseAPIDbContext context,
            IUserCacheService userCacheService)
        {
            _userManager = userManager;
            _context = context;
            _userCacheService = userCacheService;
        }

        public async Task<AccountStatus> EnforceAsync(AppUser user, CancellationToken cancellationToken = default)
        {
            if (user.Status != false || !user.DeleteDate.HasValue)
            {
                return AccountStatus.Active;
            }

            var remaining = user.DeleteDate.Value - DateTime.UtcNow;

            if (remaining <= TimeSpan.Zero)
            {
                await DeleteAsync(user, cancellationToken);
                _userCacheService.RemoveUserFromCache(user.Id);

                return new AccountStatus(true, "Account deleted");
            }

            var days = Math.Max(1, (int)Math.Ceiling(remaining.TotalDays));

            return new AccountStatus(false, $"Account will be deleted in {days} day(s).");
        }

        private async Task DeleteAsync(AppUser user, CancellationToken cancellationToken)
        {
            // A friendship and a block each point at two users, and the database refuses to
            // delete a row either of them still names. Both sides of both go first, or the
            // delete fails on a foreign key and the account can never leave.
            var friendships = await _context.UserFriendships
                .Where(friendship => friendship.UserAId == user.Id || friendship.UserBId == user.Id)
                .ToListAsync(cancellationToken);

            var blocks = await _context.UserBlocks
                .Where(block => block.BlockerId == user.Id || block.BlockedId == user.Id)
                .ToListAsync(cancellationToken);

            _context.UserFriendships.RemoveRange(friendships);
            _context.UserBlocks.RemoveRange(blocks);

            await _context.SaveChangesAsync(cancellationToken);

            var result = await _userManager.DeleteAsync(user);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    "Could not delete the account: " +
                    string.Join("; ", result.Errors.Select(error => error.Description)));
            }
        }
    }
}
