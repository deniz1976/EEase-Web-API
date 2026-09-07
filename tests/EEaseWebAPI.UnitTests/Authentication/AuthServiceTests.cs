using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.Abstractions.Token;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.Authentication;
using FluentAssertions;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Authentication
{
    public class AuthServiceTests
    {
        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenHandler _tokenHandler = Substitute.For<ITokenHandler>();
        private readonly IMailService _mail = Substitute.For<IMailService>();
        private readonly IUserAccountService _accountService = Substitute.For<IUserAccountService>();
        private readonly IAccountDeletionPolicy _deletionPolicy = Substitute.For<IAccountDeletionPolicy>();
        private readonly IVerificationCodeGenerator _codes = Substitute.For<IVerificationCodeGenerator>();
        private readonly AuthService _service;

        private readonly AppUser _alice = new()
        {
            Id = "alice-id",
            UserName = "alice",
            Email = "alice@example.com",
            EmailConfirmed = true
        };

        public AuthServiceTests()
        {
            _signInManager = Substitute.For<SignInManager<AppUser>>(
                _userManager,
                Substitute.For<IHttpContextAccessor>(),
                Substitute.For<IUserClaimsPrincipalFactory<AppUser>>(),
                null, null, null, null);

            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.UpdateAsync(Arg.Any<AppUser>()).Returns(IdentityResult.Success);
            _codes.Generate().Returns("123456");
            _deletionPolicy.EnforceAsync(Arg.Any<AppUser>()).Returns(AccountStatus.Active);

            _tokenHandler.CreateAccessToken(Arg.Any<int>(), Arg.Any<AppUser>()).Returns(
                new Token { AccessToken = "access", RefreshToken = "refresh", Expiration = DateTime.UtcNow.AddMinutes(15) });

            _service = new AuthService(
                _userManager, _signInManager, _tokenHandler, _mail, _accountService, _deletionPolicy, _codes);
        }

        [Fact]
        public async Task An_unknown_user_cannot_log_in()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.LoginAsync("nobody", "whatever", 900));
        }

        [Fact]
        public async Task A_wrong_password_is_rejected()
        {
            _signInManager.CheckPasswordSignInAsync(_alice, "wrong", false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserAuthenticationException>(
                () => _service.LoginAsync("alice", "wrong", 900));
        }

        [Fact]
        public async Task A_wrong_password_never_reaches_the_deletion_policy()
        {
            _alice.Status = false;
            _alice.DeleteDate = DateTime.UtcNow.AddDays(-1);
            _signInManager.CheckPasswordSignInAsync(_alice, "wrong", false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Failed);

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserAuthenticationException>(
                () => _service.LoginAsync("alice", "wrong", 900));

            await _deletionPolicy.DidNotReceiveWithAnyArgs().EnforceAsync(default!);
        }

        [Fact]
        public async Task An_unconfirmed_email_gets_a_fresh_verification_code()
        {
            _alice.EmailConfirmed = false;
            _signInManager.CheckPasswordSignInAsync(_alice, "OldPass1!", false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.EmailConfirmException>(
                () => _service.LoginAsync("alice", "OldPass1!", 900));

            _alice.VerificationCode.Should().Be("123456");
            _mail.Received(1).SendVerificationEmail(_alice.Email, Arg.Any<string>(), "123456");
        }

        [Fact]
        public async Task A_login_reports_the_previous_last_seen_and_then_moves_it()
        {
            var earlier = DateTime.UtcNow.AddDays(-2);
            _alice.LastSeen = earlier;
            _signInManager.CheckPasswordSignInAsync(_alice, "OldPass1!", false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);

            var body = await _service.LoginAsync("alice", "OldPass1!", 900);

            body.userInfo!.lastSeen.Should().Be(earlier);
            body.warning.Should().Be("Account is active.");
            _alice.LastSeen.Should().BeAfter(earlier);
        }

        [Fact]
        public async Task A_login_that_triggers_deletion_does_not_hand_out_a_token()
        {
            _signInManager.CheckPasswordSignInAsync(_alice, "OldPass1!", false)
                .Returns(Microsoft.AspNetCore.Identity.SignInResult.Success);
            _deletionPolicy.EnforceAsync(_alice).Returns(new AccountStatus(true, "Account deleted"));

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.LoginAsync("alice", "OldPass1!", 900));

            await _accountService.DidNotReceiveWithAnyArgs().UpdateRefreshTokenAsync(default!, default!, default);
        }

        [Fact]
        public async Task An_unknown_username_cannot_get_a_refreshed_token()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.UpdateUserGetNewToken("nobody"));
        }

        [Fact]
        public async Task A_renamed_user_gets_a_token_and_a_stored_refresh_token()
        {
            var token = await _service.UpdateUserGetNewToken("alice");

            token.AccessToken.Should().Be("access");
            await _accountService.Received(1).UpdateRefreshTokenAsync(
                "refresh", _alice, Arg.Any<TimeSpan>());
        }

        [Fact]
        public async Task An_email_that_belongs_to_nobody_is_free()
        {
            (await _service.IsEmailInUse("free@example.com")).Should().BeFalse();
        }

        [Fact]
        public async Task An_existing_external_user_is_linked_instead_of_recreated()
        {
            _userManager.FindByEmailAsync("alice@example.com").Returns(_alice);
            _userManager.AddLoginAsync(_alice, Arg.Any<UserLoginInfo>()).Returns(IdentityResult.Success);

            var token = await _service.CreateUserExternalAsync(
                null!, "alice@example.com", "Alice", new UserLoginInfo("google", "key", "Google"),
                900, "Doe", "alice", "female");

            token.AccessToken.Should().Be("access");
            await _userManager.DidNotReceiveWithAnyArgs().CreateAsync(default!);
            await _userManager.Received(1).AddLoginAsync(_alice, Arg.Any<UserLoginInfo>());
        }

        [Fact]
        public async Task A_failed_external_creation_is_reported()
        {
            _userManager.CreateAsync(Arg.Any<AppUser>())
                .Returns(IdentityResult.Failed(new IdentityError { Description = "Taken." }));

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.CreateUser.CreateUserFailedException>(
                () => _service.CreateUserExternalAsync(
                    null!, "new@example.com", "New", new UserLoginInfo("google", "key", "Google"),
                    900, "User", "new", "male"));
        }
    }
}
