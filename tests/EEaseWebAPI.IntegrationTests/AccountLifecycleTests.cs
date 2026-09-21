using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.IntegrationTests
{
    public class AccountLifecycleTests : IClassFixture<EEaseApiFactory>
    {
        private readonly EEaseApiFactory _factory;
        private readonly HttpClient _client;

        public AccountLifecycleTests(EEaseApiFactory factory)
        {
            _factory = factory;
            _client = factory.CreateClient();
        }

        [Fact]
        public async Task A_traveller_registers_signs_in_and_reads_themselves_back()
        {
            var client = await Travellers.SignedInAsync(_factory, "ada");

            var body = await Travellers.BodyAsync(await client.GetAsync("/api/users/me"));

            body.GetProperty("username").GetString().Should().Be("ada");
        }

        [Fact]
        public async Task A_name_that_is_already_taken_is_refused()
        {
            (await _client.PostAsJsonAsync("/api/users", Travellers.Registration("grace")))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var again = await _client.PostAsJsonAsync("/api/users", Travellers.Registration("grace"));

            again.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        }

        [Fact]
        public async Task Only_the_fields_that_were_sent_are_changed()
        {
            var client = await Travellers.SignedInAsync(_factory, "mary");

            (await client.PatchAsJsonAsync("/api/users/me", new { bio = "counts things" }))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var body = await Travellers.BodyAsync(await client.GetAsync("/api/users/me"));

            body.GetProperty("bio").GetString().Should().Be("counts things");
            body.GetProperty("name").GetString().Should().Be("Ada");
            body.GetProperty("surname").GetString().Should().Be("Lovelace");
        }

        [Fact]
        public async Task A_signed_in_traveller_reaches_what_is_theirs()
        {
            var client = await Travellers.SignedInAsync(_factory, "edith");

            foreach (var path in new[] { "/api/users/me/status", "/api/routes", "/api/friends", "/api/blocked-users" })
            {
                (await client.GetAsync(path)).StatusCode
                    .Should().Be(HttpStatusCode.OK, $"{path} belongs to the caller");
            }
        }

        [Fact]
        public async Task A_password_that_is_wrong_does_not_sign_anybody_in()
        {
            (await _client.PostAsJsonAsync("/api/users", Travellers.Registration("hedy")))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var login = await _client.PostAsJsonAsync(
                "/api/auth/login", new { usernameOrEmail = "hedy", password = "not-the-password" });

            login.StatusCode.Should().NotBe(HttpStatusCode.OK);
        }

        [Fact]
        public async Task A_token_that_says_nothing_is_not_a_token()
        {
            var client = _factory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", "not-a-jwt");

            (await client.GetAsync("/api/users/me")).StatusCode
                .Should().Be(HttpStatusCode.Unauthorized);
        }
    }
}
