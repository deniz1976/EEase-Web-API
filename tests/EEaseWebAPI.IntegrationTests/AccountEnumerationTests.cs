using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.IntegrationTests
{
    // Everything a stranger can reach without a token has to answer the same way for a name
    // that is registered and one that is not. Anything else is a way to ask, one name at a
    // time, who has an account here.
    public class AccountEnumerationTests : IClassFixture<EEaseApiFactory>
    {
        private readonly EEaseApiFactory _factory;
        private readonly HttpClient _client;

        public AccountEnumerationTests(EEaseApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        private async Task RegisterAsync(string username) =>
            (await _client.PostAsJsonAsync("/api/users", Travellers.Registration(username)))
                .StatusCode.Should().Be(HttpStatusCode.Created);

        private static async Task<(HttpStatusCode Status, string Message)> Describe(HttpResponseMessage response)
        {
            var body = System.Text.Json.JsonDocument.Parse(await response.Content.ReadAsStringAsync())
                .RootElement.GetProperty("body");

            return (response.StatusCode,
                body.TryGetProperty("message", out var message) ? message.GetString() ?? string.Empty : string.Empty);
        }

        [Fact]
        public async Task Signing_in_answers_the_same_for_a_name_that_exists_and_one_that_does_not()
        {
            await RegisterAsync("realuser");

            var real = await Describe(await _client.PostAsJsonAsync(
                "/api/auth/login", new { usernameOrEmail = "realuser", password = "wrong-password" }));

            var invented = await Describe(await _client.PostAsJsonAsync(
                "/api/auth/login", new { usernameOrEmail = "inventeduser", password = "wrong-password" }));

            real.Should().Be(invented);
            real.Status.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task Asking_for_a_reset_answers_the_same_either_way()
        {
            await RegisterAsync("resetuser");

            var real = await Describe(await _client.PostAsJsonAsync(
                "/api/auth/password-resets", new { emailOrUsername = "resetuser" }));

            var invented = await Describe(await _client.PostAsJsonAsync(
                "/api/auth/password-resets", new { emailOrUsername = "inventedreset" }));

            real.Should().Be(invented);
            real.Status.Should().Be(HttpStatusCode.OK);
        }
    }
}
