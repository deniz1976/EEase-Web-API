using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.ResetUserPreferences;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.User;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    public class UserPreferenceServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;
        private readonly IGeminiAIService _gemini = Substitute.For<IGeminiAIService>();
        private readonly IFriendshipService _friendships = Substitute.For<IFriendshipService>();
        private readonly UserPreferenceService _service;

        private readonly AppUser _alice = new() { Id = "alice-id", UserName = "alice" };
        private readonly AppUser _bob = new() { Id = "bob-id", UserName = "bob" };

        public UserPreferenceServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"preferences-{Guid.NewGuid():N}")
                    .Options);

            _context.Users.AddRange(_alice, _bob);
            _context.SaveChanges();

            _service = new UserPreferenceService(_context, _gemini, _friendships);
        }

        public void Dispose() => _context.Dispose();

        private void GeminiReturns(int? luxuryHotel = null, int? streetFood = null, int? museum = null)
        {
            _gemini.GetUserPreferencesFromMessage(Arg.Any<string>()).Returns(
                (new UserAccommodationPreferences { LuxuryHotelPreference = luxuryHotel },
                 new UserFoodPreferences { StreetFoodPreference = streetFood },
                 new UserPersonalization { CulturalPreference = museum }));
        }

        private async Task SeedPreferencesAsync(int score = 60)
        {
            await _context.AddRangeAsync(
                new UserAccommodationPreferences { UserId = _alice.Id, LuxuryHotelPreference = score },
                new UserFoodPreferences { UserId = _alice.Id, StreetFoodPreference = score },
                new UserPersonalization { UserId = _alice.Id, CulturalPreference = score });

            await _context.SaveChangesAsync();
        }

        [Fact]
        public async Task An_unknown_user_is_reported()
        {
            await Assert.ThrowsAsync<EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException>(
                () => _service.GetUserWithPreferencesAsync("nobody"));
        }

        [Fact]
        public async Task A_message_without_any_preference_is_refused()
        {
            GeminiReturns();

            await Assert.ThrowsAsync<UpdateUserSaveException>(
                () => _service.SetFromMessageAsync("alice", "hello"));

            _context.Set<UserFoodPreferences>().Should().BeEmpty();
        }

        [Fact]
        public async Task A_single_extracted_preference_is_enough_to_be_stored()
        {
            GeminiReturns(streetFood: 70);

            await _service.SetFromMessageAsync("alice", "I love street food");

            var food = await _context.Set<UserFoodPreferences>().SingleAsync();
            food.UserId.Should().Be(_alice.Id);
            food.StreetFoodPreference.Should().Be(70);

            _context.Set<UserAccommodationPreferences>().Should().ContainSingle();
            _context.Set<UserPersonalization>().Should().ContainSingle();
        }

        [Fact]
        public async Task Preferences_are_not_overwritten_and_the_reason_survives()
        {
            await SeedPreferencesAsync();
            GeminiReturns(streetFood: 70);

            await Assert.ThrowsAsync<UserAlreadyHasPreferencesException>(
                () => _service.SetFromMessageAsync("alice", "I love street food"));
        }

        [Fact]
        public async Task Topics_are_not_applied_on_top_of_existing_preferences()
        {
            await SeedPreferencesAsync();

            await Assert.ThrowsAsync<UserAlreadyHasPreferencesException>(
                () => _service.SetFromTopicsAsync("alice", new[] { "Luxury Stays" }));
        }

        [Fact]
        public async Task A_topic_scores_every_preference_of_its_group()
        {
            await _service.SetFromTopicsAsync("alice", new[] { "Luxury Stays" });

            var accommodation = await _context.Set<UserAccommodationPreferences>().SingleAsync();
            accommodation.LuxuryHotelPreference.Should().Be(60);
            accommodation.VillaPreference.Should().Be(60);
            accommodation.ResortPreference.Should().Be(60);
            accommodation.HostelPreference.Should().BeNull();
        }

        [Fact]
        public async Task An_unknown_topic_is_ignored_rather_than_failing()
        {
            await _service.SetFromTopicsAsync("alice", new[] { "Not A Topic" });

            _context.Set<UserAccommodationPreferences>().Should().ContainSingle();
        }

        [Fact]
        public async Task Resetting_without_preferences_is_reported()
        {
            await Assert.ThrowsAsync<ResetUserPreferencesFailedException>(
                () => _service.ResetAsync("alice"));
        }

        [Fact]
        public async Task Resetting_removes_all_three_rows_of_the_caller_only()
        {
            await SeedPreferencesAsync();
            await _context.AddAsync(new UserFoodPreferences { UserId = _bob.Id, StreetFoodPreference = 80 });
            await _context.SaveChangesAsync();

            await _service.ResetAsync("alice");

            _context.Set<UserAccommodationPreferences>().Should().BeEmpty();
            _context.Set<UserPersonalization>().Should().BeEmpty();
            (await _context.Set<UserFoodPreferences>().SingleAsync()).UserId.Should().Be(_bob.Id);
        }

        [Fact]
        public async Task Only_preferences_above_the_threshold_are_described()
        {
            await _context.AddRangeAsync(
                new UserFoodPreferences { UserId = _alice.Id, StreetFoodPreference = 46, VeganPreference = 45 },
                new UserPersonalization { UserId = _alice.Id, CulturalPreference = 90 });
            await _context.SaveChangesAsync();

            var descriptions = await _service.GetDescriptionsAsync("alice");

            descriptions.FoodPreferences.Should().ContainSingle()
                .Which.Value.Should().Be(46);
            descriptions.PersonalizationPreferences.Should().ContainSingle();
            descriptions.AccommodationPreferences.Should().BeEmpty();
        }

        [Fact]
        public async Task A_user_with_only_weak_preferences_is_reported()
        {
            await SeedPreferencesAsync(score: 10);

            await Assert.ThrowsAsync<BaseException>(() => _service.GetDescriptionsAsync("alice"));
        }

        [Fact]
        public async Task A_stranger_is_not_shown_the_preferences()
        {
            await SeedPreferencesAsync();
            _friendships.AreFriendsAsync("bob", "alice").Returns(false);

            (await _service.GetDescriptionsForViewerAsync("bob", "alice")).Should().BeNull();
        }

        [Fact]
        public async Task A_friend_is_shown_the_preferences()
        {
            await SeedPreferencesAsync();
            _friendships.AreFriendsAsync("bob", "alice").Returns(true);

            var descriptions = await _service.GetDescriptionsForViewerAsync("bob", "alice");

            descriptions.Should().NotBeNull();
            descriptions!.FoodPreferences.Should().ContainSingle();
        }

        [Fact]
        public async Task Your_own_preferences_need_no_friendship_check()
        {
            await SeedPreferencesAsync();

            (await _service.GetDescriptionsForViewerAsync("alice", "alice")).Should().NotBeNull();

            await _friendships.DidNotReceiveWithAnyArgs().AreFriendsAsync(default!, default!);
        }

        [Fact]
        public void Every_topic_group_is_offered()
        {
            var topics = _service.GetAllTopics();

            topics.AccommodationTopics.Should().Contain("Luxury Stays");
            topics.FoodTopics.Should().NotBeEmpty();
            topics.TravelTopics.Should().NotBeEmpty();
        }
    }
}
