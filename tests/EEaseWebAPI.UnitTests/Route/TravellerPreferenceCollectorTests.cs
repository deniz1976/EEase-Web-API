using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class TravellerPreferenceCollectorTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;

        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IFriendshipService _friendships = Substitute.For<IFriendshipService>();
        private readonly TravellerPreferenceCollector _collector;

        private readonly AppUser _alice = new() { Id = "alice-id", UserName = "alice" };
        private readonly AppUser _bob = new() { Id = "bob-id", UserName = "bob" };
        private readonly AppUser _mallory = new() { Id = "mallory-id", UserName = "mallory" };

        public TravellerPreferenceCollectorTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"travellers-{Guid.NewGuid():N}")
                    .Options);

            _context.Users.AddRange(_alice, _bob, _mallory);
            _context.SaveChanges();

            _userManager.FindByNameAsync("bob").Returns(_bob);
            _userManager.FindByNameAsync("mallory").Returns(_mallory);

            _friendships.AreFriendsAsync("alice", "bob").Returns(true);
            _friendships.AreFriendsAsync("alice", "mallory").Returns(false);

            _collector = new TravellerPreferenceCollector(_userManager, _friendships, _context);
        }

        public void Dispose() => _context.Dispose();

        private void GivenFullPreferences(string userId)
        {
            _context.Add(new UserAccommodationPreferences { UserId = userId, LuxuryHotelPreference = 80 });
            _context.Add(new UserFoodPreferences { UserId = userId });
            _context.Add(new UserPersonalization { UserId = userId });
            _context.SaveChanges();
        }

        [Fact]
        public async Task The_requesting_user_is_always_a_traveller()
        {
            GivenFullPreferences(_alice.Id);

            var collected = await _collector.CollectAsync(_alice, null);

            collected.Should().ContainSingle();
            collected[0].Accommodation!.UserId.Should().Be(_alice.Id);
        }

        [Fact]
        public async Task A_user_who_filled_in_only_one_questionnaire_still_counts()
        {
            _context.Add(new UserFoodPreferences { UserId = _alice.Id });
            _context.SaveChanges();

            var collected = await _collector.CollectAsync(_alice, null);

            collected.Should().ContainSingle();
            collected[0].Accommodation.Should().BeNull();
            collected[0].Food.Should().NotBeNull();
            collected[0].Personalization.Should().BeNull();
        }

        [Fact]
        public async Task A_user_with_no_preferences_at_all_is_left_out()
        {
            var collected = await _collector.CollectAsync(_alice, null);

            collected.Should().BeEmpty();
        }

        [Fact]
        public async Task A_friend_travelling_along_contributes_their_preferences()
        {
            GivenFullPreferences(_alice.Id);
            GivenFullPreferences(_bob.Id);

            var collected = await _collector.CollectAsync(_alice, new[] { "bob" });

            collected.Should().HaveCount(2);
        }

        [Fact]
        public async Task Someone_who_is_not_a_friend_cannot_be_dragged_into_the_route()
        {
            GivenFullPreferences(_alice.Id);
            GivenFullPreferences(_mallory.Id);

            var collected = await _collector.CollectAsync(_alice, new[] { "mallory" });

            collected.Should().ContainSingle();
        }

        [Fact]
        public async Task Naming_yourself_as_a_friend_does_not_count_you_twice()
        {
            GivenFullPreferences(_alice.Id);

            var collected = await _collector.CollectAsync(_alice, new[] { "ALICE", "alice" });

            collected.Should().ContainSingle();
        }

        [Fact]
        public async Task The_same_friend_listed_twice_is_only_one_traveller()
        {
            GivenFullPreferences(_alice.Id);
            GivenFullPreferences(_bob.Id);

            var collected = await _collector.CollectAsync(_alice, new[] { "bob", "bob" });

            collected.Should().HaveCount(2);
        }

        [Fact]
        public async Task A_blank_friend_name_is_ignored()
        {
            GivenFullPreferences(_alice.Id);

            var collected = await _collector.CollectAsync(_alice, new[] { "   " });

            collected.Should().ContainSingle();
        }
    }
}
