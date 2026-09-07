using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PreferenceFeedbackServiceTests : IDisposable
    {
        private const string UserId = "alice-id";

        private readonly EEaseAPIDbContext _context;
        private readonly IGeminiAIService _gemini = Substitute.For<IGeminiAIService>();
        private readonly PreferenceFeedbackService _service;

        public PreferenceFeedbackServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"feedback-{Guid.NewGuid():N}")
                    .Options);

            _service = new PreferenceFeedbackService(_context, _gemini);
        }

        public void Dispose() => _context.Dispose();

        private void GeminiPicks(params string[] names) =>
            _gemini.AnalyzePlacePreferencesAsync(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<List<string>>())
                .Returns(names.ToList());

        private async Task<UserFoodPreferences> SeedFoodAsync(int? streetFood = null)
        {
            var preferences = new UserFoodPreferences { UserId = UserId, StreetFoodPreference = streetFood };

            await _context.AddAsync(preferences);
            await _context.SaveChangesAsync();

            return preferences;
        }

        private async Task<UserPersonalization> SeedPersonalizationAsync(int? cultural = null)
        {
            var preferences = new UserPersonalization { UserId = UserId, CulturalPreference = cultural };

            await _context.AddAsync(preferences);
            await _context.SaveChangesAsync();

            return preferences;
        }

        private static Dinner Restaurant() =>
            new() { Id = Guid.NewGuid(), PrimaryType = "restaurant", Rating = 4.5 };

        private static Place Attraction() =>
            new() { Id = Guid.NewGuid(), PrimaryType = "museum", Rating = 4.7 };

        [Fact]
        public async Task A_user_without_preferences_is_asked_to_introduce_themselves()
        {
            GeminiPicks("StreetFoodPreference");

            await Assert.ThrowsAsync<BaseException>(
                () => _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: true));
        }

        [Fact]
        public async Task An_unknown_place_type_is_refused()
        {
            await Assert.ThrowsAsync<InvalidPlaceTypeException>(
                () => _service.ApplyAsync(UserId, Restaurant(), "spaceship", liked: true));
        }

        [Fact]
        public async Task Liking_a_restaurant_raises_the_matching_food_preference()
        {
            var preferences = await SeedFoodAsync(streetFood: 40);
            GeminiPicks("StreetFoodPreference");

            var result = await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: true);

            preferences.StreetFoodPreference.Should().Be(55);
            result.Category.Should().Be("food");
            result.Changes["StreetFoodPreference"].Should().Be(15);
        }

        [Fact]
        public async Task Disliking_a_restaurant_lowers_the_matching_food_preference()
        {
            var preferences = await SeedFoodAsync(streetFood: 40);
            GeminiPicks("StreetFoodPreference");

            var result = await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: false);

            preferences.StreetFoodPreference.Should().Be(30);
            result.Changes["StreetFoodPreference"].Should().Be(-10);
        }

        [Fact]
        public async Task A_like_followed_by_a_dislike_lands_back_near_the_starting_score()
        {
            var preferences = await SeedFoodAsync(streetFood: 40);
            GeminiPicks("StreetFoodPreference");

            await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: true);
            await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: false);

            preferences.StreetFoodPreference.Should().BeInRange(38, 42);
        }

        [Fact]
        public async Task Liking_the_same_thing_over_and_over_cannot_pass_the_maximum()
        {
            var preferences = await SeedFoodAsync(streetFood: 0);
            GeminiPicks("StreetFoodPreference");

            for (var i = 0; i < 40; i++)
            {
                await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: true);
            }

            preferences.StreetFoodPreference.Should().Be(100);
        }

        [Fact]
        public async Task An_unset_preference_starts_from_zero()
        {
            var preferences = await SeedFoodAsync();
            GeminiPicks("StreetFoodPreference");

            await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: true);

            preferences.StreetFoodPreference.Should().Be(25);
        }

        [Fact]
        public async Task A_preference_the_model_invented_is_ignored()
        {
            var preferences = await SeedFoodAsync(streetFood: 40);
            GeminiPicks("MadeUpPreference", "UserId");

            var result = await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: true);

            result.HasChanges.Should().BeFalse();
            preferences.StreetFoodPreference.Should().Be(40);
        }

        [Fact]
        public async Task A_model_that_matches_nothing_leaves_the_profile_alone()
        {
            await SeedFoodAsync(streetFood: 40);
            _gemini.AnalyzePlacePreferencesAsync(
                    Arg.Any<string>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<List<string>>())
                .Returns(new List<string>());

            var result = await _service.ApplyAsync(UserId, Restaurant(), "dinner", liked: true);

            result.Should().Be(EEaseWebAPI.Application.DTOs.Route.PreferenceFeedbackResult.None);
        }

        [Fact]
        public async Task A_touristic_place_updates_the_personalization_profile()
        {
            var preferences = await SeedPersonalizationAsync(cultural: 20);
            GeminiPicks("CulturalPreference");

            var result = await _service.ApplyAsync(UserId, Attraction(), "firstplace", liked: true);

            result.Category.Should().Be("personalization");
            preferences.CulturalPreference.Should().Be(40);
        }

        [Fact]
        public async Task The_change_summary_signs_the_direction()
        {
            await SeedPersonalizationAsync(cultural: 20);
            GeminiPicks("CulturalPreference");

            var liked = await _service.ApplyAsync(UserId, Attraction(), "place", liked: true);
            liked.Describe().Should().Contain("(+20)");

            var disliked = await _service.ApplyAsync(UserId, Attraction(), "place", liked: false);
            disliked.Describe().Should().Contain("(-10)");
        }
    }
}
