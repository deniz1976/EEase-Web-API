using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.IntegrationTests
{
    public class ErrorEnvelopeTests : IClassFixture<EEaseApiFactory>
    {
        private readonly HttpClient _client;

        public ErrorEnvelopeTests(EEaseApiFactory factory) => _client = factory.CreateClient();

        private static async Task<JsonElement> BodyOf(HttpResponseMessage response)
        {
            var envelope = JsonDocument.Parse(await response.Content.ReadAsStringAsync()).RootElement;

            envelope.TryGetProperty("header", out var header).Should().BeTrue();
            header.GetProperty("success").GetBoolean().Should().BeFalse();
            header.GetProperty("enumStatusCode").GetInt32().Should().BeGreaterThan(0);

            return envelope.GetProperty("body");
        }

        [Fact]
        public async Task A_request_without_a_token_is_refused_in_the_usual_envelope()
        {
            var response = await _client.GetAsync("/api/users/me");

            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

            var body = await BodyOf(response);

            body.GetProperty("statusCode").GetInt32().Should().Be(401);
            body.GetProperty("message").GetString().Should().NotBeNullOrWhiteSpace();
        }

        [Fact]
        public async Task A_request_that_breaks_a_rule_names_the_field_that_broke_it()
        {
            var response = await _client.GetAsync("/api/users/email-confirmation");

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await BodyOf(response);

            body.GetProperty("statusCode").GetInt32().Should().Be(400);
            body.GetProperty("errors").GetProperty("EmailOrUsername")
                .EnumerateArray().Should().NotBeEmpty();
        }

        [Fact]
        public async Task The_caller_is_answered_in_the_language_they_asked_for()
        {
            var turkish = new HttpRequestMessage(HttpMethod.Get, "/api/users/email-confirmation");
            turkish.Headers.Add("Accept-Language", "tr");

            var english = new HttpRequestMessage(HttpMethod.Get, "/api/users/email-confirmation");
            english.Headers.Add("Accept-Language", "en");

            var inTurkish = await BodyOf(await _client.SendAsync(turkish));
            var inEnglish = await BodyOf(await _client.SendAsync(english));

            inTurkish.GetProperty("message").GetString()
                .Should().NotBe(inEnglish.GetProperty("message").GetString());
        }

        [Fact]
        public async Task An_unknown_path_is_a_plain_not_found_rather_than_an_envelope()
        {
            var response = await _client.GetAsync("/api/there-is-no-such-thing");

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task A_body_the_endpoint_cannot_read_is_refused_in_the_usual_envelope()
        {
            var response = await _client.PostAsJsonAsync(
                "/api/users", new { name = "", surname = "", username = "", email = "not-an-email" });

            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var body = await BodyOf(response);

            body.GetProperty("errors").EnumerateObject().Should().NotBeEmpty();
        }
    }
}
