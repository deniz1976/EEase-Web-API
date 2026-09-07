using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Authentication
{
    public class AccountDeletionPolicyTests
    {
        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IUserCacheService _cache = Substitute.For<IUserCacheService>();
        private readonly AccountDeletionPolicy _policy;

        public AccountDeletionPolicyTests()
        {
            _userManager.DeleteAsync(Arg.Any<AppUser>()).Returns(IdentityResult.Success);
            _policy = new AccountDeletionPolicy(_userManager, _cache);
        }

        private static AppUser User(bool? status = true, DateTime? deleteDate = null) =>
            new() { Id = "alice-id", UserName = "alice", Status = status, DeleteDate = deleteDate };

        [Fact]
        public async Task An_active_account_is_left_alone()
        {
            var status = await _policy.EnforceAsync(User());

            status.IsDeleted.Should().BeFalse();
            status.Message.Should().Be("Account is active.");
            await _userManager.DidNotReceiveWithAnyArgs().DeleteAsync(default!);
        }

        [Fact]
        public async Task A_deactivated_account_without_a_deletion_date_survives()
        {
            (await _policy.EnforceAsync(User(status: false))).IsDeleted.Should().BeFalse();
        }

        [Fact]
        public async Task An_active_account_with_a_stale_deletion_date_is_not_deleted()
        {
            var status = await _policy.EnforceAsync(
                User(status: true, deleteDate: DateTime.UtcNow.AddDays(-30)));

            status.IsDeleted.Should().BeFalse();
            await _userManager.DidNotReceiveWithAnyArgs().DeleteAsync(default!);
        }

        [Fact]
        public async Task A_grace_period_that_has_run_out_deletes_the_account()
        {
            var user = User(status: false, deleteDate: DateTime.UtcNow.AddMinutes(-1));

            var status = await _policy.EnforceAsync(user);

            status.IsDeleted.Should().BeTrue();
            status.Message.Should().Be("Account deleted");
            await _userManager.Received(1).DeleteAsync(user);
            _cache.Received(1).RemoveUserFromCache(user.Id);
        }

        [Fact]
        public async Task A_pending_deletion_reports_the_days_that_are_left()
        {
            var status = await _policy.EnforceAsync(
                User(status: false, deleteDate: DateTime.UtcNow.AddDays(3).AddHours(1)));

            status.IsDeleted.Should().BeFalse();
            status.Message.Should().Be("Account will be deleted in 4 day(s).");
        }

        [Fact]
        public async Task The_last_hours_still_count_as_one_day()
        {
            var status = await _policy.EnforceAsync(
                User(status: false, deleteDate: DateTime.UtcNow.AddMinutes(30)));

            status.Message.Should().Be("Account will be deleted in 1 day(s).");
        }
    }
}
