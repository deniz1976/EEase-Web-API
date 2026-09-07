using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;

namespace EEaseWebAPI.Persistence.Services.Authentication
{
    public sealed class AccountDeletionPolicy : IAccountDeletionPolicy
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IUserCacheService _userCacheService;

        public AccountDeletionPolicy(UserManager<AppUser> userManager, IUserCacheService userCacheService)
        {
            _userManager = userManager;
            _userCacheService = userCacheService;
        }

        public async Task<AccountStatus> EnforceAsync(AppUser user)
        {
            if (user.Status != false || !user.DeleteDate.HasValue)
            {
                return AccountStatus.Active;
            }

            var remaining = user.DeleteDate.Value - DateTime.UtcNow;

            if (remaining <= TimeSpan.Zero)
            {
                await _userManager.DeleteAsync(user);
                _userCacheService.RemoveUserFromCache(user.Id);

                return new AccountStatus(true, "Account deleted");
            }

            var days = Math.Max(1, (int)Math.Ceiling(remaining.TotalDays));

            return new AccountStatus(false, $"Account will be deleted in {days} day(s).");
        }
    }
}
