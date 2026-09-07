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
        private static GeminiKeyManager CreateManager(
            int requestsPerMinute,
            int quotaCooldownSeconds,
            params string[] keys) =>
            new(Options.Create(new GeminiOptions
            {
                ApiKeys = keys,
                RequestsPerMinutePerKey = requestsPerMinute,
                QuotaCooldownSeconds = quotaCooldownSeconds
            }));

        private static GeminiKeyManager CreateManager(int requestsPerMinute, params string[] keys) =>
            CreateManager(requestsPerMinute, 60, keys);

        [Fact]
        public async Task Throws_a_clear_error_when_no_key_is_configured()
        {
            var manager = CreateManager(60);

            await Assert.ThrowsAsync<GeminiAPIKeyNotFoundException>(() => manager.AcquireKeyAsync());
        }

        [Fact]
        public async Task Blank_keys_are_excluded_from_the_pool()
        {
            var manager = CreateManager(60, "", "  ", "valid-key");

            var key = await manager.AcquireKeyAsync();

            key.Should().Be("valid-key");
        }

        [Fact]
        public async Task Spreads_load_across_the_pool_instead_of_draining_the_first_key()
        {
            var manager = CreateManager(6000, "first", "second", "third");

            var keys = new List<string>();
            for (var i = 0; i < 6; i++)
            {
                keys.Add(await manager.AcquireKeyAsync());
            }

            keys.Should().Equal("first", "second", "third", "first", "second", "third");
        }

        [Fact]
        public async Task Lets_a_key_burst_up_to_its_per_minute_quota()
        {
            var manager = CreateManager(10, "single-key");

            var stopwatch = Stopwatch.StartNew();
            for (var i = 0; i < 10; i++)
            {
                (await manager.AcquireKeyAsync()).Should().Be("single-key");
            }
            stopwatch.Stop();

            stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
        }

        [Fact]
        public async Task Waits_once_a_single_key_has_spent_its_quota()
        {
            var manager = CreateManager(600, "single-key");

            for (var i = 0; i < 600; i++)
            {
                await manager.AcquireKeyAsync();
            }

            var stopwatch = Stopwatch.StartNew();
            var key = await manager.AcquireKeyAsync();
            stopwatch.Stop();

            key.Should().Be("single-key");
            stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(30);
        }

        [Fact]
        public async Task Concurrent_callers_never_share_the_same_quota_slot()
        {
            const int slots = 50;
            var manager = CreateManager(slots, "first", "second");

            var handed = await Task.WhenAll(Enumerable
                .Range(0, slots * 2)
                .Select(_ => Task.Run(() => manager.AcquireKeyAsync())));

            handed.Should().HaveCount(slots * 2);
            handed.Count(key => key == "first").Should().Be(slots);
            handed.Count(key => key == "second").Should().Be(slots);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(200));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => manager.AcquireKeyAsync(cts.Token));
        }

        [Fact]
        public async Task A_key_reported_as_quota_exceeded_is_benched_for_the_cooldown()
        {
            var manager = CreateManager(6000, quotaCooldownSeconds: 600, keys: new[] { "first", "second" });

            manager.ReportQuotaExceeded("first");

            for (var i = 0; i < 5; i++)
            {
                (await manager.AcquireKeyAsync()).Should().Be("second");
            }
        }

        [Fact]
        public async Task Waiting_for_a_key_honours_cancellation()
        {
            var manager = CreateManager(1, quotaCooldownSeconds: 600, keys: new[] { "only-key" });

            await manager.AcquireKeyAsync();
            manager.ReportQuotaExceeded("only-key");

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(100));

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => manager.AcquireKeyAsync(cts.Token));
        }
    }
}
