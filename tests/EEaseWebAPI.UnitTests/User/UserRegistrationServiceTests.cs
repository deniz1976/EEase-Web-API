using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Services.Authentication;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Exceptions.CreateUser;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.User;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    public class UserRegistrationServiceTests
    {
        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IHeaderService _headers = Substitute.For<IHeaderService>();
        private readonly IMailService _mail = Substitute.For<IMailService>();
        private readonly IUserCacheService _cache = Substitute.For<IUserCacheService>();
        private readonly IVerificationCodeGenerator _codes = Substitute.For<IVerificationCodeGenerator>();
        private readonly UserRegistrationService _service;

        private readonly AppUser _alice = new()
        {
            Id = "alice-id",
            UserName = "alice",
            Email = "alice@example.com"
        };

        public UserRegistrationServiceTests()
        {
            _userManager.UpdateAsync(Arg.Any<AppUser>()).Returns(IdentityResult.Success);
            _userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);
            _codes.Generate().Returns("123456");

            _service = new UserRegistrationService(_userManager, _headers, _mail, _cache, _codes);
        }

        private static CreateUser Registration(DateOnly? bornDate = null, string name = "deniz", string surname = "mutlu") =>
            new()
            {
                Email = "new@example.com",
                Username = "new",
                Password = "NewPass1!",
                Name = name,
                Surname = surname,
                Gender = "Male",
                BornDate = bornDate
            };

        [Fact]
        public async Task An_email_that_is_taken_blocks_registration()
        {
            _userManager.FindByEmailAsync("new@example.com").Returns(_alice);

            await Assert.ThrowsAsync<CreateUserFailedException>(() => _service.CreateAsync(Registration()));
        }

        [Fact]
        public async Task A_username_that_is_taken_blocks_registration()
        {
            _userManager.FindByNameAsync("new").Returns(_alice);

            await Assert.ThrowsAsync<CreateUserFailedException>(() => _service.CreateAsync(Registration()));
        }

        [Fact]
        public async Task A_twelve_year_old_cannot_register()
        {
            var bornDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-12);

            await Assert.ThrowsAsync<CreateUserFailedException>(
                () => _service.CreateAsync(Registration(bornDate)));
        }

        [Fact]
        public async Task A_thirteen_year_old_can_register()
        {
            var bornDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-13);

            await _service.CreateAsync(Registration(bornDate));

            await _userManager.Received(1).CreateAsync(Arg.Any<AppUser>(), "NewPass1!");
        }

        [Fact]
        public async Task A_birthday_that_has_not_come_around_yet_still_counts_as_a_year_younger()
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            UserRegistrationService.AgeOn(today.AddYears(-13).AddDays(1)).Should().Be(12);
            UserRegistrationService.AgeOn(today.AddYears(-13)).Should().Be(13);
        }

        [Fact]
        public async Task A_new_account_is_mailed_its_verification_code()
        {
            AppUser? created = null;
            _userManager.CreateAsync(Arg.Do<AppUser>(user => created = user), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            await _service.CreateAsync(Registration());

            created!.VerificationCode.Should().Be("123456");
            _mail.Received(1).SendVerificationEmail("new@example.com", Arg.Any<string>(), "123456");
        }

        [Fact]
        public async Task Names_are_capitalised_the_Turkish_way()
        {
            AppUser? created = null;
            _userManager.CreateAsync(Arg.Do<AppUser>(user => created = user), Arg.Any<string>())
                .Returns(IdentityResult.Success);

            await _service.CreateAsync(Registration(name: "IŞIK", surname: "mutlu"));

            created!.Name.Should().Be("Işık");
            created.Surname.Should().Be("Mutlu");
        }

        [Fact]
        public async Task Resending_to_an_unknown_address_reports_the_missing_user()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.SendVerificationEmailAgain("nobody@example.com"));
        }

        [Fact]
        public async Task Resending_to_an_empty_address_reports_the_missing_user()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.SendVerificationEmailAgain("  "));
        }

        [Fact]
        public async Task An_already_confirmed_account_is_not_mailed_again()
        {
            _alice.EmailConfirmed = true;
            _userManager.FindByEmailAsync("alice@example.com").Returns(_alice);

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.EmailConfirmException>(
                () => _service.SendVerificationEmailAgain("alice@example.com"));
        }

        [Fact]
        public async Task Resending_replaces_the_stored_code()
        {
            _alice.VerificationCode = "999999";
            _userManager.FindByEmailAsync("alice@example.com").Returns(_alice);

            await _service.SendVerificationEmailAgain("alice@example.com");

            _alice.VerificationCode.Should().Be("123456");
            _mail.Received(1).SendVerificationEmail("alice@example.com", Arg.Any<string>(), "123456");
        }

        [Fact]
        public async Task The_right_code_confirms_the_address_and_burns_the_code()
        {
            _alice.VerificationCode = "123456";
            _userManager.FindByEmailAsync("alice").Returns(_alice);

            (await _service.EmailConfirm("123456", "alice")).Should().BeTrue();

            _alice.EmailConfirmed.Should().BeTrue();
            _alice.VerificationCode.Should().BeNull();
            _cache.Received(1).AddOrUpdateUserInCache(_alice);
        }

        [Fact]
        public async Task A_wrong_code_does_not_confirm_the_address()
        {
            _alice.VerificationCode = "123456";
            _userManager.FindByEmailAsync("alice").Returns(_alice);

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.EmailConfirmException>(
                () => _service.EmailConfirm("000000", "alice"));

            _alice.EmailConfirmed.Should().BeFalse();
        }

        [Fact]
        public async Task An_account_with_no_pending_code_cannot_be_confirmed_by_a_null_code()
        {
            _userManager.FindByEmailAsync("alice").Returns(_alice);

            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.EmailConfirmException>(
                () => _service.EmailConfirm(null!, "alice"));
        }

        [Fact]
        public async Task Confirmation_state_can_be_read_back()
        {
            _alice.EmailConfirmed = true;
            _userManager.FindByEmailAsync("alice").Returns(_alice);

            (await _service.CheckEmailConfirmed("alice")).Should().BeTrue();
        }
    }
}
