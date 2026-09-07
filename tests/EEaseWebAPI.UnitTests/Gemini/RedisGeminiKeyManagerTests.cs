using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Persistence.Services.Gemini;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using StackExchange.Redis;
using Xunit;

namespace EEaseWebAPI.UnitTests.Gemini
{
    public class RedisGeminiKeyManagerTests
    {
        private static string? ConnectionString =>
            Environment.GetEnvironmentVariable("REDIS_TEST_CONNECTION");

        private static (RedisGeminiKeyManager Manager, IConnectionMultiplexer Connection)? TryCreate(
            int requestsPerMinute,
            int quotaCooldownSeconds,
            params string[] apiKeys)
        {
            if (string.IsNullOrWhiteSpace(ConnectionString))
            {
                return null;
            }

            var configuration = ConfigurationOptions.Parse(ConnectionString);
            configuration.AbortOnConnectFail = false;

            var connection = ConnectionMultiplexer.Connect(configuration);

            var geminiOptions = Options.Create(new GeminiOptions
            {
                ApiKeys = apiKeys,
                RequestsPerMinutePerKey = requestsPerMinute,
                QuotaCooldownSeconds = quotaCooldownSeconds
            });

            var redisOptions = Options.Create(new RedisOptions
            {
                ConnectionString = ConnectionString!,
                InstanceName = $"eease-test:{Guid.NewGuid():N}"
            });

            var manager = new RedisGeminiKeyManager(
                connection,
                new GeminiKeyManager(geminiOptions),
                geminiOptions,
                redisOptions,
                NullLogger<RedisGeminiKeyManager>.Instance);

            return (manager, connection);
        }

        [Fact]
        public async Task Shares_one_quota_across_managers_the_way_two_instances_would()
        {
            var first = TryCreate(10, 600, "first-key", "second-key");
            if (first is null)
            {
                return;
            }

            using var connection = first.Value.Connection;

            var handed = await Task.WhenAll(Enumerable
                .Range(0, 20)
                .Select(_ => Task.Run(() => first.Value.Manager.AcquireKeyAsync())));

            handed.Count(key => key == "first-key").Should().Be(10);
            handed.Count(key => key == "second-key").Should().Be(10);

            using var cts = new CancellationTokenSource(TimeSpan.FromMilliseconds(300));
            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => first.Value.Manager.AcquireKeyAsync(cts.Token));
        }

        [Fact]
        public async Task A_quota_breach_benches_the_key_for_every_instance()
        {
            var created = TryCreate(6000, 600, "first-key", "second-key");
            if (created is null)
            {
                return;
            }

            using var connection = created.Value.Connection;
            var manager = created.Value.Manager;

            manager.ReportQuotaExceeded("first-key");

            for (var i = 0; i < 5; i++)
            {
                (await manager.AcquireKeyAsync()).Should().Be("second-key");
            }
        }
    }
}
