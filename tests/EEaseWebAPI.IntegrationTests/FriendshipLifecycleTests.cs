using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.IntegrationTests
{
    public class FriendshipLifecycleTests : IClassFixture<EEaseApiFactory>
    {
        private readonly EEaseApiFactory _factory;

        public FriendshipLifecycleTests(EEaseApiFactory factory) => _factory = factory;

        private static IEnumerable<string> UsernamesIn(JsonElement body, string collection, string field) =>
            body.GetProperty(collection).EnumerateArray()
                .Select(entry => entry.GetProperty(field).GetString()!);

        [Fact]
        public async Task Two_travellers_become_friends_and_stop_being_friends()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adaf");
            var grace = await Travellers.SignedInAsync(_factory, "gracef");

            (await ada.PostAsync("/api/friend-requests/gracef", null))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var pendingForAda = await Travellers.BodyAsync(await ada.GetAsync("/api/friend-requests/gracef"));
            pendingForAda.GetProperty("status").GetInt32().Should().Be(1);

            var waiting = await Travellers.BodyAsync(await grace.GetAsync("/api/friend-requests"));
            UsernamesIn(waiting, "pendingRequests", "requesterUsername").Should().Contain("adaf");

            (await grace.PutAsJsonAsync("/api/friend-requests/adaf", 1))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var adasFriends = await Travellers.BodyAsync(await ada.GetAsync("/api/friends"));
            UsernamesIn(adasFriends, "friends", "username").Should().Contain("gracef");

            var gracesFriends = await Travellers.BodyAsync(await grace.GetAsync("/api/friends"));
            UsernamesIn(gracesFriends, "friends", "username").Should().Contain("adaf");

            (await ada.DeleteAsync("/api/friends/gracef"))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var afterwards = await Travellers.BodyAsync(await ada.GetAsync("/api/friends"));
            UsernamesIn(afterwards, "friends", "username").Should().NotContain("gracef");
        }

        [Fact]
        public async Task A_request_can_be_withdrawn_before_it_is_answered()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adaw");
            var grace = await Travellers.SignedInAsync(_factory, "gracew");

            (await ada.PostAsync("/api/friend-requests/gracew", null))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            (await ada.DeleteAsync("/api/friend-requests/gracew"))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var waiting = await Travellers.BodyAsync(await grace.GetAsync("/api/friend-requests"));
            UsernamesIn(waiting, "pendingRequests", "requesterUsername").Should().NotContain("adaw");
        }

        [Fact]
        public async Task A_traveller_is_blocked_and_unblocked()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adab");
            await Travellers.SignedInAsync(_factory, "graceb");

            (await ada.PostAsync("/api/blocked-users/graceb", null))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var blocked = await Travellers.BodyAsync(await ada.GetAsync("/api/blocked-users"));
            UsernamesIn(blocked, "blockedUsers", "username").Should().Contain("graceb");

            (await ada.DeleteAsync("/api/blocked-users/graceb"))
                .StatusCode.Should().Be(HttpStatusCode.OK);

            var afterwards = await Travellers.BodyAsync(await ada.GetAsync("/api/blocked-users"));
            UsernamesIn(afterwards, "blockedUsers", "username").Should().NotContain("graceb");
        }

        [Fact]
        public async Task Asking_about_yourself_is_answered_rather_than_refused()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adas");

            var body = await Travellers.BodyAsync(await ada.GetAsync("/api/friend-requests/adas"));

            body.GetProperty("status").GetInt32().Should().Be(5);
        }

        [Fact]
        public async Task A_request_to_somebody_who_does_not_exist_is_refused()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adan");

            (await ada.PostAsync("/api/friend-requests/nobody-at-all", null))
                .StatusCode.Should().Be(HttpStatusCode.NotFound);
        }
    }
}
