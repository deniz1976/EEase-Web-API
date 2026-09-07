using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Services.Gemini;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Gemini
{
    public class GeminiAIServiceTests
    {
        private readonly IGeminiApiClient _apiClient = Substitute.For<IGeminiApiClient>();
        private readonly GeminiAIService _service;

        public GeminiAIServiceTests()
        {
            _service = new GeminiAIService(_apiClient);
        }

        private void Answers(params string[] responses)
        {
            var index = 0;

            _apiClient.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(_ => responses[Math.Min(index++, responses.Length - 1)]);
        }

        private static string Itinerary(string accommodation = "Hotel One", string description = "A good day") =>
            $$"""
            {"AnonymousDayList":[{
              "DayDescription": "{{description}}",
              "AccomodationPlaceName": "{{accommodation}}",
              "BreakfastPlaceName": "Cafe",
              "LunchPlaceName": "Bistro",
              "DinnerPlaceName": "Tavern",
              "FirstPlaceName": "Museum",
              "SecondPlaceName": "Park",
              "ThirdPlaceName": "Tower",
              "AfterDinnerPlaceName": "Bar",
              "ApproxPrice": "100"
            }]}
            """;

        [Fact]
        public async Task A_usable_itinerary_is_returned_on_the_first_call()
        {
            Answers(Itinerary());

            var days = await _service.CreateRouteAnonymous("Lisbon", 1, null, null);

            days.Should().ContainSingle();
            await _apiClient.Received(1).GenerateContentAsync(
                Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task A_code_fenced_answer_is_still_read()
        {
            Answers("```json\n" + Itinerary() + "\n```");

            (await _service.CreateRouteAnonymous("Lisbon", 1, null, null)).Should().ContainSingle();
        }

        [Fact]
        public async Task A_placeholder_venue_is_rejected_and_the_model_is_asked_again()
        {
            Answers(Itinerary(accommodation: "N/A"), Itinerary());

            var days = await _service.CreateRouteAnonymous("Lisbon", 1, null, null);

            days.Should().ContainSingle();
            await _apiClient.Received(2).GenerateContentAsync(
                Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task A_model_that_never_answers_usefully_gives_up_after_three_attempts()
        {
            Answers(Itinerary(accommodation: "NULL"));

            await Assert.ThrowsAsync<GeminiAPIResponseParseException>(
                () => _service.CreateRouteAnonymous("Lisbon", 1, null, null));

            await _apiClient.Received(3).GenerateContentAsync(
                Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Unreadable_json_is_retried_and_then_reported()
        {
            Answers("not json at all");

            await Assert.ThrowsAsync<GeminiAPIResponseParseException>(
                () => _service.CreateRouteAnonymous("Lisbon", 1, null, null));

            await _apiClient.Received(3).GenerateContentAsync(
                Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task An_empty_day_list_is_retried()
        {
            Answers("""{"AnonymousDayList":[]}""", Itinerary());

            (await _service.CreateRouteAnonymous("Lisbon", 1, null, null)).Should().ContainSingle();
        }

        [Fact]
        public async Task A_destination_is_required()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(
                () => _service.CreateRouteAnonymous("  ", 1, null, null));
        }

        [Fact]
        public async Task A_user_without_maxed_preferences_still_gets_a_prompt()
        {
            string? captured = null;

            _apiClient.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    captured = call.Arg<string>();
                    return "[]";
                });

            await _service.CreateCustomRoute(
                "Lisbon", 2, new DateOnly(2026, 6, 1), null, PRICE_LEVEL.PRICE_LEVEL_MODERATE,
                new UserAccommodationPreferences(),
                new UserFoodPreferences(),
                new UserPersonalization());

            captured.Should().NotBeNull();
            captured!.Should().Contain("Comfortable Hotel name");
            captured.Should().Contain("next day of");
        }

        [Fact]
        public async Task Maxed_preferences_are_spread_over_the_days()
        {
            string? captured = null;

            _apiClient.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    captured = call.Arg<string>();
                    return "[]";
                });

            await _service.CreateCustomRoute(
                "Lisbon", 1, new DateOnly(2026, 6, 1), null, PRICE_LEVEL.PRICE_LEVEL_MODERATE,
                new UserAccommodationPreferences { LuxuryHotelPreference = 100 },
                new UserFoodPreferences { StreetFoodPreference = 100, SeafoodPreference = 100 },
                new UserPersonalization { CulturalPreference = 100 });

            captured!.Should().Contain("LuxuryHotelPreference Hotel name");
            captured.Should().Contain("StreetFoodPreference");
            captured.Should().Contain("SeafoodPreference");
            captured.Should().Contain("CulturalPreference");
        }

        [Fact]
        public async Task A_strong_but_not_maxed_preference_still_reaches_the_prompt()
        {
            string? captured = null;

            _apiClient.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    captured = call.Arg<string>();
                    return "[]";
                });

            await _service.CreateCustomRoute(
                "Lisbon", 1, new DateOnly(2026, 6, 1), null, PRICE_LEVEL.PRICE_LEVEL_MODERATE,
                new UserAccommodationPreferences { BoutiqueHotelPreference = 72 },
                new UserFoodPreferences { StreetFoodPreference = 65 },
                new UserPersonalization { CulturalPreference = 61 });

            captured!.Should().Contain("BoutiqueHotelPreference Hotel name");
            captured.Should().Contain("StreetFoodPreference");
            captured.Should().Contain("CulturalPreference");
        }

        [Fact]
        public async Task The_strongest_preference_is_offered_first()
        {
            string? captured = null;

            _apiClient.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    captured = call.Arg<string>();
                    return "[]";
                });

            await _service.CreateCustomRoute(
                "Lisbon", 1, new DateOnly(2026, 6, 1), null, PRICE_LEVEL.PRICE_LEVEL_MODERATE,
                new UserAccommodationPreferences { BoutiqueHotelPreference = 65, LuxuryHotelPreference = 95 },
                new UserFoodPreferences(),
                new UserPersonalization());

            captured!.Should().Contain("LuxuryHotelPreference Hotel name");
        }

        [Fact]
        public async Task A_weak_preference_is_not_treated_as_a_favourite()
        {
            string? captured = null;

            _apiClient.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    captured = call.Arg<string>();
                    return "[]";
                });

            await _service.CreateCustomRoute(
                "Lisbon", 1, new DateOnly(2026, 6, 1), null, PRICE_LEVEL.PRICE_LEVEL_MODERATE,
                new UserAccommodationPreferences { HostelPreference = 20 },
                new UserFoodPreferences(),
                new UserPersonalization());

            captured!.Should().Contain("Comfortable Hotel name");
        }

        [Fact]
        public async Task An_empty_message_is_refused_before_calling_the_model()
        {
            await Assert.ThrowsAsync<GeminiInvalidMessageException>(
                () => _service.GetUserPreferencesFromMessage("   "));

            await _apiClient.DidNotReceiveWithAnyArgs().GenerateContentAsync(default!);
        }

        [Fact]
        public async Task Weather_carries_the_date_that_was_asked_for()
        {
            Answers("""{"Degree":21,"Description":"sunny","Warning":"Wear sunscreen."}""");

            var weather = await _service.GetWeatherForDateAsync("Lisbon", new DateOnly(2026, 6, 1), new TimeOnly(9, 0));

            weather.Degree.Should().Be(21);
            weather.Date.Should().Be(new DateOnly(2026, 6, 1));
        }

        [Fact]
        public async Task Place_preferences_outside_the_offered_list_are_dropped()
        {
            Answers("""["CulturalPreference","MadeUpPreference"]""");

            var preferences = await _service.AnalyzePlacePreferencesAsync(
                "Museum", "museum", "A museum", new List<string> { "CulturalPreference" });

            preferences.Should().Equal("CulturalPreference");
        }

        [Fact]
        public async Task An_answer_wrapped_in_prose_still_yields_the_array()
        {
            Answers("""Sure! Here you go: ["CulturalPreference"] Hope that helps.""");

            var preferences = await _service.AnalyzePlacePreferencesAsync(
                "Museum", "museum", "A museum", new List<string> { "CulturalPreference" });

            preferences.Should().Equal("CulturalPreference");
        }

        [Fact]
        public async Task An_answer_with_no_matching_preference_is_reported()
        {
            Answers("""["MadeUpPreference"]""");

            await Assert.ThrowsAsync<GeminiAPIResponseParseException>(
                () => _service.AnalyzePlacePreferencesAsync(
                    "Museum", "museum", "A museum", new List<string> { "CulturalPreference" }));
        }
    }
}
