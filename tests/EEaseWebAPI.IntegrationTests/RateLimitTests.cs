using System.Net;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.IntegrationTests
{
    public class RateLimitTests : IClassFixture<ThrottledApiFactory>
    {
        private readonly HttpClient _client;

        public RateLimitTests(ThrottledApiFactory factory) => _client = factory.CreateClient();

        [Fact]
        public async Task A_sensitive_endpoint_stops_answering_after_its_limit()
        {
            var responses = new List<HttpResponseMessage>();

            for (var attempt = 0; attempt < 15; attempt++)
            {
                responses.Add(await _client.GetAsync("/api/users/email-availability?email=a@b.com"));
            }

            var refused = responses.FirstOrDefault(
                response => response.StatusCode == HttpStatusCode.TooManyRequests);

            refused.Should().NotBeNull("the sensitive policy allows ten requests a minute");
            refused!.Headers.RetryAfter.Should().NotBeNull();

            var body = await BodyOf(refused);

            body.GetProperty("statusCode").GetInt32().Should().Be(429);
        }

        private static async Task<JsonElement> BodyOf(HttpResponseMessage response)
        {
            var envelope = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

            envelope.GetProperty("header").GetProperty("success").GetBoolean().Should().BeFalse();

            return envelope.GetProperty("body");
        }
    }
}
