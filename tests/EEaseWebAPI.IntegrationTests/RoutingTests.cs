using System.Net;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.IntegrationTests
{
    public class RoutingTests : IClassFixture<EEaseApiFactory>
    {
        private readonly HttpClient _client;

        public RoutingTests(EEaseApiFactory factory) =>
            _client = factory.CreateClient(new()
            {
                AllowAutoRedirect = false
            });

        [Theory]
        [InlineData("/api/users/me")]
        [InlineData("/api/users/adatest")]
        [InlineData("/api/users/by-id/some-id")]
        [InlineData("/api/users/me/photo")]
        [InlineData("/api/users/adatest/photo")]
        [InlineData("/api/users/me/preferences")]
        [InlineData("/api/users/me/status")]
        [InlineData("/api/routes")]
        [InlineData("/api/routes/liked")]
        [InlineData("/api/routes/11111111-1111-1111-1111-111111111111")]
        [InlineData("/api/routes/11111111-1111-1111-1111-111111111111/likes/me")]
        [InlineData("/api/countries")]
        [InlineData("/api/currencies")]
        [InlineData("/api/friends")]
        [InlineData("/api/friend-requests")]
        [InlineData("/api/friend-requests/adatest")]
        [InlineData("/api/blocked-users")]
        public async Task An_endpoint_behind_a_token_is_reached_and_refuses(string path)
        {
            var response = await _client.GetAsync(path);

            response.StatusCode.Should().Be(
                HttpStatusCode.Unauthorized,
                "a 404 would mean the route template never matched");
        }

        [Theory]
        [InlineData("/api/routes/not-a-guid")]
        [InlineData("/api/routes/not-a-guid/likes/me")]
        public async Task A_route_id_that_is_not_a_guid_never_reaches_a_handler(string path)
        {
            var response = await _client.GetAsync(path);

            response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("/api/preference-topics")]
        [InlineData("/api/cities?search=rom")]
        public async Task An_endpoint_open_to_everybody_answers_without_a_token(string path)
        {
            var response = await _client.GetAsync(path);

            response.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        [Theory]
        [InlineData("GET", "/api/users/me")]
        [InlineData("PATCH", "/api/users/me")]
        [InlineData("DELETE", "/api/users/me")]
        [InlineData("GET", "/api/users/me/preferences")]
        [InlineData("PUT", "/api/users/me/preferences")]
        [InlineData("DELETE", "/api/users/me/preferences")]
        [InlineData("GET", "/api/friend-requests/adatest")]
        [InlineData("POST", "/api/friend-requests/adatest")]
        [InlineData("PUT", "/api/friend-requests/adatest")]
        [InlineData("DELETE", "/api/friend-requests/adatest")]
        public async Task One_path_answers_to_several_methods(string method, string path)
        {
            var response = await _client.SendAsync(new HttpRequestMessage(new HttpMethod(method), path));

            response.StatusCode.Should().NotBe(HttpStatusCode.MethodNotAllowed);
            response.StatusCode.Should().NotBe(HttpStatusCode.NotFound);
        }

        [Theory]
        [InlineData("DELETE", "/api/users/email-confirmation")]
        [InlineData("DELETE", "/api/users/me/deletion-request")]
        [InlineData("GET", "/api/places/likes")]
        [InlineData("PUT", "/api/routes/guest")]
        public async Task A_method_an_endpoint_does_not_answer_is_refused(string method, string path)
        {
            var wrong = new HttpRequestMessage(new HttpMethod(method), path);

            (await _client.SendAsync(wrong)).StatusCode
                .Should().BeOneOf(HttpStatusCode.MethodNotAllowed, HttpStatusCode.NotFound);
        }

        [Fact]
        public async Task The_api_still_describes_itself()
        {
            var response = await _client.GetAsync("/swagger/v1/swagger.json");

            response.StatusCode.Should().Be(HttpStatusCode.OK);
            (await response.Content.ReadAsStringAsync()).Should().Contain("/api/users/me");
        }
    }
}
