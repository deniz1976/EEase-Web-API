using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.DeleteUser;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.User;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    public class UserAccountServiceTests
    {
        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IMailService _mail = Substitute.For<IMailService>();
        private readonly IVerificationCodeGenerator _codes = Substitute.For<IVerificationCodeGenerator>();
        private readonly UserAccountService _service;

        private readonly AppUser _alice = new()
        {
            Id = "alice-id",
            UserName = "alice",
            Email = "alice@example.com",
            Status = true
        };

        public UserAccountServiceTests()
        {
            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.UpdateAsync(Arg.Any<AppUser>()).Returns(IdentityResult.Success);
            _codes.Generate().Returns("123456");

            _service = new UserAccountService(_userManager, _mail, _codes);
        }

        private void WithDeleteCode(string code = "123456", TimeSpan? age = null)
        {
            _alice.DeleteCode = code;
            _alice.DeleteCodeExpiration = DateTime.UtcNow.Add(age ?? TimeSpan.FromMinutes(10));
        }

        [Fact]
        public async Task An_unknown_user_cannot_request_deletion()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.RequestDeletionAsync("nobody"));
        }

        [Fact]
        public async Task An_active_user_is_mailed_a_delete_code()
        {
            var outcome = await _service.RequestDeletionAsync("alice");

            outcome.Should().Be(DeleteRequestOutcome.CodeSent);
            _alice.DeleteCodeExpiration.Should().BeAfter(DateTime.UtcNow);
            _mail.Received(1).SendDeleteCodeEmail(_alice.Email, Arg.Any<string>(), "123456");
        }

        [Fact]
        public async Task A_pending_deletion_is_cancelled_instead_of_restarted()
        {
            _alice.Status = false;
            _alice.DeleteDate = DateTime.UtcNow.AddDays(3);
            WithDeleteCode();

            var outcome = await _service.RequestDeletionAsync("alice");

            outcome.Should().Be(DeleteRequestOutcome.Reactivated);
            _alice.Status.Should().BeTrue();
            _alice.DeleteDate.Should().BeNull();
            _alice.DeleteCode.Should().BeNull();
            _mail.DidNotReceiveWithAnyArgs().SendDeleteCodeEmail(default!, default!, default!);
        }

        [Fact]
        public async Task A_correct_code_schedules_the_deletion_a_week_out()
        {
            WithDeleteCode();

            var message = await _service.ConfirmDeletionAsync("alice", "123456");

            message.Should().Be("Code is correct, account will be deleted in 7 days");
            _alice.Status.Should().BeFalse();
            _alice.DeleteDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
            _alice.DeleteCode.Should().BeNull();
        }

        [Fact]
        public async Task A_wrong_code_is_refused()
        {
            WithDeleteCode();

            await Assert.ThrowsAsync<DeleteUserCodeNotCorrectException>(
                () => _service.ConfirmDeletionAsync("alice", "000000"));

            _alice.Status.Should().BeTrue();
        }

        [Fact]
        public async Task An_expired_code_is_refused_and_discarded()
        {
            WithDeleteCode(age: TimeSpan.FromMinutes(-1));

            await Assert.ThrowsAsync<DeleteUserCodeNotCorrectException>(
                () => _service.ConfirmDeletionAsync("alice", "123456"));

            _alice.DeleteCode.Should().BeNull();
            _alice.Status.Should().BeTrue();
        }

        [Fact]
        public async Task A_code_cannot_be_replayed_after_it_was_used()
        {
            WithDeleteCode();
            await _service.ConfirmDeletionAsync("alice", "123456");

            await Assert.ThrowsAsync<UserStatusAlreadyFalseException>(
                () => _service.ConfirmDeletionAsync("alice", "123456"));
        }

        [Fact]
        public async Task Deletion_cannot_be_confirmed_without_ever_asking_for_a_code()
        {
            await Assert.ThrowsAsync<DeleteUserCodeNotCorrectException>(
                () => _service.ConfirmDeletionAsync("alice", "123456"));
        }

        [Fact]
        public async Task A_user_with_no_status_reads_as_active()
        {
            _alice.Status = null;

            var status = await _service.StatusCheck("alice");

            status.status.Should().BeTrue();
            status.message.Should().Be("User is active");
        }

        [Fact]
        public async Task A_deactivated_user_reads_as_passive()
        {
            _alice.Status = false;

            (await _service.StatusCheck("alice")).message.Should().Be("User is passive");
        }

        [Fact]
        public async Task A_refresh_token_expires_after_the_lifetime_it_was_given()
        {
            await _service.UpdateRefreshTokenAsync("refresh", _alice, TimeSpan.FromDays(7));

            _alice.RefreshToken.Should().Be("refresh");
            _alice.RefreshTokenEndDate.Should().BeCloseTo(DateTime.UtcNow.AddDays(7), TimeSpan.FromMinutes(1));
        }
    }
}
