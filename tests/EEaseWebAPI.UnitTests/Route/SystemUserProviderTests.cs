using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class SystemUserProviderTests
    {
        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly SystemUserProvider _provider;

        public SystemUserProviderTests() =>
            _provider = new SystemUserProvider(_userManager, NullLogger<SystemUserProvider>.Instance);

        [Fact]
        public async Task The_account_that_already_exists_is_the_one_handed_back()
        {
            var existing = new AppUser { Id = "system-id", UserName = SystemUserProvider.SystemUserName };

            _userManager.FindByNameAsync(SystemUserProvider.SystemUserName).Returns(existing);

            (await _provider.GetOrCreateAsync()).Should().BeSameAs(existing);

            await _userManager.DidNotReceive().CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>());
        }

        [Fact]
        public async Task An_account_is_created_the_first_time_a_guest_asks_for_a_route()
        {
            _userManager.FindByNameAsync(SystemUserProvider.SystemUserName).Returns((AppUser?)null);
            _userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>()).Returns(IdentityResult.Success);

            var created = await _provider.GetOrCreateAsync();

            created.UserName.Should().Be(SystemUserProvider.SystemUserName);
            created.EmailConfirmed.Should().BeTrue();
        }

        [Fact]
        public async Task Nobody_can_sign_in_as_the_account_that_owns_anonymous_routes()
        {
            AppUser? created = null;
            string? password = null;

            _userManager.FindByNameAsync(SystemUserProvider.SystemUserName).Returns((AppUser?)null);
            _userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>())
                .Returns(call =>
                {
                    created = call.Arg<AppUser>();
                    password = call.Arg<string>();
                    return IdentityResult.Success;
                });

            await _provider.GetOrCreateAsync();

            created!.LockoutEnabled.Should().BeTrue();
            created.LockoutEnd.Should().Be(DateTimeOffset.MaxValue);
            password.Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task Two_asks_in_a_row_do_not_make_two_passwords_the_same()
        {
            var passwords = new List<string>();

            _userManager.FindByNameAsync(SystemUserProvider.SystemUserName).Returns((AppUser?)null);
            _userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>())
                .Returns(call =>
                {
                    passwords.Add(call.Arg<string>());
                    return IdentityResult.Success;
                });

            await _provider.GetOrCreateAsync();
            await _provider.GetOrCreateAsync();

            passwords.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task A_refusal_says_what_identity_objected_to()
        {
            _userManager.FindByNameAsync(SystemUserProvider.SystemUserName).Returns((AppUser?)null);
            _userManager.CreateAsync(Arg.Any<AppUser>(), Arg.Any<string>())
                .Returns(IdentityResult.Failed(new IdentityError { Description = "Name is taken" }));

            var asking = () => _provider.GetOrCreateAsync();

            await asking.Should().ThrowAsync<InvalidOperationException>()
                .WithMessage("*Name is taken*");
        }
    }
}
