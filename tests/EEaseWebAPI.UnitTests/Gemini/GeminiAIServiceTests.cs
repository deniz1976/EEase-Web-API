using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Domain.Entities.Identity;
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

        [Fact]
        public async Task An_empty_message_is_refused_before_calling_the_model()
        {
            await Assert.ThrowsAsync<GeminiInvalidMessageException>(
                () => _service.GetUserPreferencesFromMessage("   "));

            await _apiClient.DidNotReceiveWithAnyArgs().GenerateContentAsync(default!);
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
