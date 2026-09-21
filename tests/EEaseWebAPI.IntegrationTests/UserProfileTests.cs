using System.Text.Json;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.IntegrationTests
{
    public class UserProfileTests : IClassFixture<EEaseApiFactory>
    {
        private readonly EEaseApiFactory _factory;

        public UserProfileTests(EEaseApiFactory factory) => _factory = factory;

        [Fact]
        public async Task A_traveller_looked_up_by_id_and_by_name_is_the_same_traveller()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adap");
            await Travellers.SignedInAsync(_factory, "gracep");

            var byName = await Travellers.BodyAsync(await ada.GetAsync("/api/users/gracep"));

            var id = byName.GetProperty("id").GetString();
            id.Should().NotBeNullOrWhiteSpace();

            var byId = await Travellers.BodyAsync(await ada.GetAsync($"/api/users/by-id/{id}"));

            byId.GetRawText().Should().Be(
                byName.GetRawText(),
                "the two only differ in how the traveller was found");
        }

        [Fact]
        public async Task Looking_somebody_up_says_where_the_two_of_them_stand()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adar");
            await Travellers.SignedInAsync(_factory, "gracer");

            var before = await Travellers.BodyAsync(await ada.GetAsync("/api/users/gracer"));

            before.GetProperty("isFriend").GetBoolean().Should().BeFalse();
            before.TryGetProperty("friendRequestStatus", out _).Should().BeTrue();

            (await ada.PostAsync("/api/friend-requests/gracer", null))
                .StatusCode.Should().Be(System.Net.HttpStatusCode.Created);

            var afterwards = await Travellers.BodyAsync(await ada.GetAsync("/api/users/gracer"));

            afterwards.GetProperty("friendRequestStatus").GetInt32().Should().Be(1);
        }

        [Fact]
        public async Task A_traveller_who_does_not_exist_is_reported_either_way()
        {
            var ada = await Travellers.SignedInAsync(_factory, "adaq");

            (await ada.GetAsync("/api/users/nobody-at-all")).StatusCode
                .Should().Be(System.Net.HttpStatusCode.NotFound);

            (await ada.GetAsync("/api/users/by-id/not-a-real-id")).StatusCode
                .Should().Be(System.Net.HttpStatusCode.NotFound);
        }
    }
}
