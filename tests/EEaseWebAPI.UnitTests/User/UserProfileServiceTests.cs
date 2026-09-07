using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
using EEaseWebAPI.Application.Exceptions.UpdateUserCountry;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Domain.Entities.Currency;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.User;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    public class UserProfileServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;

        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly ICityService _cities = Substitute.For<ICityService>();
        private readonly ICurrencyService _currencies = Substitute.For<ICurrencyService>();
        private readonly IUserCacheService _cache = Substitute.For<IUserCacheService>();
        private readonly IFriendshipService _friendships = Substitute.For<IFriendshipService>();
        private readonly UserProfileService _service;

        private readonly AppUser _alice = new()
        {
            Id = "alice-id",
            UserName = "alice",
            Email = "alice@example.com",
            Name = "Alice",
            Surname = "Doe",
            Currency = "TRY",
            Country = "Turkey",
            BornDate = new DateOnly(1990, 1, 1)
        };

        private readonly AppUser _bob = new() { Id = "bob-id", UserName = "bob", Email = "bob@example.com" };

        public UserProfileServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"profile-{Guid.NewGuid():N}")
                    .Options);

            _context.Users.AddRange(_alice, _bob);
            _context.SaveChanges();

            _userManager.Users.Returns(_ => _context.Users);
            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.FindByNameAsync("bob").Returns(_bob);
            _userManager.FindByIdAsync("bob-id").Returns(_bob);
            _userManager.UpdateAsync(Arg.Any<AppUser>()).Returns(IdentityResult.Success);

            _cities.GetAllCountries().Returns(new List<string> { "Turkey", "Portugal" });
            _currencies.GetCurrenciesAsync().Returns(new List<AllWorldCurrencies>
            {
                new() { AlphabeticCode = "TRY" },
                new() { AlphabeticCode = "EUR" }
            });
            _friendships.GetRelationshipAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new UserRelationship(ProfileVisibilityStatus.LimitedAccess, FriendRequestStatus.NoRequest));

            _service = new UserProfileService(_userManager, _cities, _currencies, _cache, _friendships);
        }

        public void Dispose() => _context.Dispose();

        private static UpdateUserCommandRequest Update(
            string user = "alice", string? username = null, string? name = null,
            string? surname = null, string? gender = null, string? bio = null, DateOnly? bornDate = null) =>
            new()
            {
                user = user,
                Username = username,
                Name = name,
                Surname = surname,
                Gender = gender,
                bio = bio,
                BornDate = bornDate
            };

        [Fact]
        public async Task An_unknown_user_has_no_profile()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.GetUserInfoQuery("nobody"));
        }

        [Fact]
        public async Task A_profile_carries_the_stored_details()
        {
            var info = await _service.GetUserInfoQuery("alice");

            info.username.Should().Be("alice");
            info.email.Should().Be("alice@example.com");
            info.id.Should().Be("alice-id");
        }

        [Fact]
        public async Task Keeping_your_own_username_is_not_a_clash()
        {
            await _service.UpdateUser(Update(username: "alice"));

            _alice.UserName.Should().Be("alice");
        }

        [Fact]
        public async Task Taking_somebody_elses_username_is_refused()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUser(Update(username: "bob")));
        }

        [Fact]
        public async Task An_anonymous_caller_cannot_update_a_profile()
        {
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _service.UpdateUser(Update(user: "  ")));
        }

        [Fact]
        public async Task A_one_letter_name_is_refused()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUser(Update(name: "A")));
        }

        [Fact]
        public async Task An_over_long_bio_is_refused()
        {
            await Assert.ThrowsAsync<ArgumentException>(
                () => _service.UpdateUser(Update(bio: new string('x', 81))));
        }

        [Fact]
        public async Task An_empty_bio_is_allowed()
        {
            await _service.UpdateUser(Update(bio: ""));

            _alice.Bio.Should().Be("");
        }

        [Fact]
        public async Task An_unknown_gender_is_refused()
        {
            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUser(Update(gender: "Other")));
        }

        [Fact]
        public async Task A_twelve_year_old_cannot_set_that_birth_date()
        {
            var bornDate = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(-12);

            await Assert.ThrowsAsync<ArgumentException>(() => _service.UpdateUser(Update(bornDate: bornDate)));
        }

        [Fact]
        public async Task A_valid_update_reaches_the_cache()
        {
            await _service.UpdateUser(Update(name: "Alicia", surname: "Doe"));

            _alice.Name.Should().Be("Alicia");
            _cache.Received(1).UpdateUserAttributesInCache("alice-id", null, "Alicia", "Doe");
        }

        [Fact]
        public async Task A_country_outside_the_list_is_refused()
        {
            await Assert.ThrowsAsync<InvalidCountryException>(() => _service.UpdateUserCountry("alice", "Atlantis"));
        }

        [Fact]
        public async Task A_known_country_is_stored()
        {
            (await _service.UpdateUserCountry("alice", "portugal")).Should().BeTrue();

            _alice.Country.Should().Be("portugal");
        }

        [Fact]
        public async Task An_unknown_currency_is_refused()
        {
            await Assert.ThrowsAsync<UpdateUserSaveException>(() => _service.UpdateUserCurrency("alice", "XYZ"));
        }

        [Fact]
        public async Task A_known_currency_is_stored_even_when_the_cache_was_cold()
        {
            (await _service.UpdateUserCurrency("alice", "eur")).Should().BeTrue();

            _alice.Currency.Should().Be("eur");
            await _currencies.Received(1).GetCurrenciesAsync();
        }

        [Fact]
        public async Task A_user_without_a_currency_is_reported_clearly()
        {
            _alice.Currency = null;

            await Assert.ThrowsAsync<UpdateUserSaveException>(() => _service.GetUserCurrencyAsync("alice"));
        }

        [Fact]
        public async Task Setting_a_photo_for_an_unknown_user_is_refused()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.SetUserPhoto("nobody", "photo.png"));
        }

        [Fact]
        public async Task A_stored_photo_reaches_the_cache()
        {
            (await _service.SetUserPhoto("alice", "photo.png")).Should().BeTrue();

            _alice.PhotoPath.Should().Be("photo.png");
            _cache.Received(1).UpdateUserAttributesInCache("alice-id", photoUrl: "photo.png");
        }

        [Fact]
        public async Task A_limited_profile_hides_the_private_fields()
        {
            var (info, visibility) = await _service.GetUserInfoByNameAsync("bob", "alice");

            visibility.Should().Be(ProfileVisibilityStatus.LimitedAccess);
            info.email.Should().BeNull();
            info.borndate.Should().BeNull();
            info.name.Should().Be("Alice");
        }

        [Fact]
        public async Task A_full_access_profile_shows_the_private_fields()
        {
            _friendships.GetRelationshipAsync("bob", "alice")
                .Returns(new UserRelationship(ProfileVisibilityStatus.FullAccess, FriendRequestStatus.AlreadyFriends));

            var (info, _) = await _service.GetUserInfoByNameAsync("bob", "alice");

            info.email.Should().Be("alice@example.com");
            info.borndate.Should().Be(new DateOnly(1990, 1, 1));
        }

        [Fact]
        public async Task A_profile_can_be_reached_by_id()
        {
            var (info, _) = await _service.GetUserInfoByIdAsync("alice", "bob-id");

            info.username.Should().Be("bob");
        }
    }
}
