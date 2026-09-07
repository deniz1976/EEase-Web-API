using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Exceptions.ChangePassword;
using EEaseWebAPI.Application.Exceptions.ResetPassword;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Authentication
{
    public class PasswordServiceTests
    {
        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IMailService _mail = Substitute.For<IMailService>();
        private readonly IVerificationCodeGenerator _codes = Substitute.For<IVerificationCodeGenerator>();
        private readonly PasswordService _service;

        private readonly AppUser _alice = new()
        {
            Id = "alice-id",
            UserName = "alice",
            Email = "alice@example.com"
        };

        public PasswordServiceTests()
        {
            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.UpdateAsync(Arg.Any<AppUser>()).Returns(IdentityResult.Success);
            _codes.Generate().Returns("123456");

            _service = new PasswordService(_userManager, _mail, _codes);
        }

        private void WithActiveCode(string code = "123456", int attempts = 0, TimeSpan? age = null)
        {
            _alice.ResetPasswordCode = code;
            _alice.ResetPasswordCodeExpiration = DateTime.UtcNow.Add(age ?? TimeSpan.FromMinutes(10));
            _alice.ResetPasswordCodeAttempts = attempts;
        }

        [Fact]
        public async Task An_unknown_user_cannot_request_a_reset_code()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.SendResetCodeAsync("nobody"));
        }

        [Fact]
        public async Task A_reset_code_is_mailed_and_given_an_expiry()
        {
            await _service.SendResetCodeAsync("alice");

            _alice.ResetPasswordCode.Should().Be("123456");
            _alice.ResetPasswordCodeExpiration.Should().BeAfter(DateTime.UtcNow);
            _mail.Received(1).SendResetPasswordEmail(_alice.Email, Arg.Any<string>(), "123456");
        }

        [Fact]
        public async Task Requesting_a_new_code_clears_the_earlier_attempts()
        {
            WithActiveCode(attempts: 4);

            await _service.SendResetCodeAsync("alice");

            _alice.ResetPasswordCodeAttempts.Should().Be(0);
        }

        [Fact]
        public async Task The_right_code_is_accepted()
        {
            WithActiveCode();

            (await _service.VerifyResetCodeAsync("alice", "123456")).Should().BeTrue();
        }

        [Fact]
        public async Task A_wrong_code_is_counted_against_the_attempt_budget()
        {
            WithActiveCode();

            await Assert.ThrowsAsync<ResetPasswordCodeNotCorrectException>(
                () => _service.VerifyResetCodeAsync("alice", "000000"));

            _alice.ResetPasswordCodeAttempts.Should().Be(1);
        }

        [Fact]
        public async Task Guessing_is_stopped_after_five_wrong_codes()
        {
            WithActiveCode(attempts: 5);

            await Assert.ThrowsAsync<ResetPasswordCodeExpiredException>(
                () => _service.VerifyResetCodeAsync("alice", "123456"));

            _alice.ResetPasswordCode.Should().BeNull();
        }

        [Fact]
        public async Task An_expired_code_is_refused_and_discarded()
        {
            WithActiveCode(age: TimeSpan.FromMinutes(-1));

            await Assert.ThrowsAsync<ResetPasswordCodeExpiredException>(
                () => _service.VerifyResetCodeAsync("alice", "123456"));

            _alice.ResetPasswordCode.Should().BeNull();
            _alice.ResetPasswordCodeExpiration.Should().BeNull();
        }

        [Fact]
        public async Task A_user_who_never_asked_for_a_reset_cannot_use_a_code()
        {
            await Assert.ThrowsAsync<ResetPasswordCodeNotCorrectException>(
                () => _service.VerifyResetCodeAsync("alice", "123456"));
        }

        [Fact]
        public async Task A_code_belonging_to_another_account_is_refused()
        {
            var bob = new AppUser
            {
                Id = "bob-id",
                UserName = "bob",
                ResetPasswordCode = "654321",
                ResetPasswordCodeExpiration = DateTime.UtcNow.AddMinutes(10)
            };

            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.FindByNameAsync("bob").Returns(bob);
            WithActiveCode();

            await Assert.ThrowsAsync<ResetPasswordCodeNotCorrectException>(
                () => _service.ResetPasswordAsync("alice", "654321", "NewPass1!"));
        }

        [Fact]
        public async Task A_successful_reset_burns_the_code()
        {
            WithActiveCode();
            _userManager.GeneratePasswordResetTokenAsync(_alice).Returns("token");
            _userManager.ResetPasswordAsync(_alice, "token", "NewPass1!").Returns(IdentityResult.Success);

            await _service.ResetPasswordAsync("alice", "123456", "NewPass1!");

            _alice.ResetPasswordCode.Should().BeNull();
            _alice.ResetPasswordCodeExpiration.Should().BeNull();
            _alice.ResetPasswordCodeAttempts.Should().Be(0);
        }

        [Fact]
        public async Task A_failed_reset_keeps_the_code_usable()
        {
            WithActiveCode();
            _userManager.GeneratePasswordResetTokenAsync(_alice).Returns("token");
            _userManager.ResetPasswordAsync(_alice, "token", "weak")
                .Returns(IdentityResult.Failed(new IdentityError { Description = "Password too short." }));

            await Assert.ThrowsAsync<PasswordChangeException>(
                () => _service.ResetPasswordAsync("alice", "123456", "weak"));

            _alice.ResetPasswordCode.Should().Be("123456");
        }

        [Fact]
        public async Task Changing_a_password_to_the_same_value_is_refused()
        {
            await Assert.ThrowsAsync<SamePasswordsException>(
                () => _service.ChangePasswordAsync("alice", "Same1!", "Same1!"));
        }

        [Fact]
        public async Task A_wrong_current_password_blocks_the_change()
        {
            _userManager.CheckPasswordAsync(_alice, "wrong").Returns(false);

            await Assert.ThrowsAsync<InvalidPasswordException>(
                () => _service.ChangePasswordAsync("alice", "wrong", "NewPass1!"));
        }

        [Fact]
        public async Task An_empty_new_password_only_confirms_the_current_one()
        {
            _userManager.CheckPasswordAsync(_alice, "OldPass1!").Returns(true);

            var message = await _service.ChangePasswordAsync("alice", "OldPass1!", "");

            message.Should().Contain("Please provide a new password");
            await _userManager.DidNotReceiveWithAnyArgs().ChangePasswordAsync(default!, default!, default!);
        }

        [Fact]
        public async Task A_valid_change_goes_through()
        {
            _userManager.CheckPasswordAsync(_alice, "OldPass1!").Returns(true);
            _userManager.ChangePasswordAsync(_alice, "OldPass1!", "NewPass1!").Returns(IdentityResult.Success);

            (await _service.ChangePasswordAsync("alice", "OldPass1!", "NewPass1!"))
                .Should().Be("Password changed successfully.");
        }
    }
}
