using System.Diagnostics;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Persistence.Services.Gemini;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.Gemini
{
    public class GeminiKeyManagerTests
    {
        private static GeminiKeyManager CreateManager(int requestsPerMinute, params string[] keys) =>
            new(Options.Create(new GeminiOptions
            {
                ApiKeys = keys,
                RequestsPerMinutePerKey = requestsPerMinute
            }));

        [Fact]
        public async Task Throws_a_clear_error_when_no_key_is_configured()
        {
            var manager = CreateManager(60);

            await Assert.ThrowsAsync<GeminiAPIKeyNotFoundException>(() => manager.GetAvailableApiKey());
        }

        [Fact]
        public async Task Blank_keys_are_excluded_from_the_pool()
        {
            var manager = CreateManager(60, "", "  ", "valid-key");

            var key = await manager.GetAvailableApiKey();

            key.Should().Be("valid-key");
        }

        [Fact]
        public async Task Returns_a_free_key_instead_of_one_just_used()
        {
            var manager = CreateManager(1, "first", "second");

            var first = await manager.GetAvailableApiKey();
            await manager.MarkKeyAsUsed(first);

            var second = await manager.GetAvailableApiKey();

            second.Should().NotBe(first);
        }

        [Fact]
        public async Task Hands_back_the_same_key_immediately_while_under_the_limit()
        {
            var manager = CreateManager(6000, "single-key");

            var first = await manager.GetAvailableApiKey();
            await manager.MarkKeyAsUsed(first);

            var stopwatch = Stopwatch.StartNew();
            var second = await manager.GetAvailableApiKey();
            stopwatch.Stop();

            second.Should().Be("single-key");
            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        }

        [Fact]
        public async Task Waits_once_a_single_key_reaches_its_limit()
        {
            var manager = CreateManager(300, "single-key");

            await manager.MarkKeyAsUsed("single-key");

            var stopwatch = Stopwatch.StartNew();
            var key = await manager.GetAvailableApiKey();
            stopwatch.Stop();

            key.Should().Be("single-key");

            stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(100);
        }

        [Fact]
        public async Task ReleaseKey_does_not_make_a_key_immediately_available()
        {
            var manager = CreateManager(1, "first", "second");

            var first = await manager.GetAvailableApiKey();
            await manager.MarkKeyAsUsed(first);
            await manager.ReleaseKey(first);

            var next = await manager.GetAvailableApiKey();

            next.Should().NotBe(first);
        }
    }
}
