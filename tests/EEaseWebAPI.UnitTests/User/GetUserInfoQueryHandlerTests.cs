using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoById;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoByName;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.DTOs.User;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    /// <summary>
    /// Both handlers read a profile and, when the viewer is allowed to see it, the owner's
    /// preferences. The preference service was declared but never injected, so that second
    /// step threw a NullReferenceException and the endpoint answered 500 for every profile
    /// the viewer was actually allowed to read.
    /// </summary>
    public class GetUserInfoQueryHandlerTests
    {
        private readonly IHeaderService _headers = Substitute.For<IHeaderService>();
        private readonly IUserProfileService _profiles = Substitute.For<IUserProfileService>();
        private readonly IUserPreferenceService _preferences = Substitute.For<IUserPreferenceService>();

        public GetUserInfoQueryHandlerTests()
        {
            _headers.HeaderCreate(Arg.Any<int>(), Arg.Any<bool>(), Arg.Any<DateTime?>())
                .Returns(new Header { Success = true });

            _preferences.GetDescriptionsForViewerAsync(Arg.Any<string>(), Arg.Any<string>())
                .Returns(new GetUserPreferenceDescriptionsBody
                {
                    FoodPreferences = { new PreferenceDetail { Description = "Italian", Value = 80 } }
                });
        }

        private static GetUserInfo Profile(string username) => new()
        {
            username = username,
            name = "Marco",
            surname = "Rossi"
        };

        [Fact]
        public async Task A_profile_read_by_id_carries_the_preferences_of_a_friend()
        {
            _profiles.GetUserInfoByIdAsync("viewer", "target-id")
                .Returns((Profile("target"), ProfileVisibilityStatus.FullAccess));

            var handler = new GetUserInfoByIdQueryHandler(_headers, _profiles, _preferences);

            var response = await handler.Handle(
                new GetUserInfoByIdQueryRequest { username = "viewer", userId = "target-id" },
                CancellationToken.None);

            response.Body!.FoodPreferences.Should().ContainSingle();
        }

        [Fact]
        public async Task A_profile_read_by_name_carries_the_preferences_of_a_friend()
        {
            _profiles.GetUserInfoByNameAsync("viewer", "target")
                .Returns((Profile("target"), ProfileVisibilityStatus.FullAccess));

            var handler = new GetUserInfoByNameQueryHandler(_headers, _profiles, _preferences);

            var response = await handler.Handle(
                new GetUserInfoByNameQueryRequest { username = "viewer", targetUsername = "target" },
                CancellationToken.None);

            response.Body!.FoodPreferences.Should().ContainSingle();
        }

        [Fact]
        public async Task A_stranger_is_told_to_send_a_friend_request_instead()
        {
            _profiles.GetUserInfoByIdAsync("viewer", "target-id")
                .Returns((Profile("target"), ProfileVisibilityStatus.LimitedAccess));

            var handler = new GetUserInfoByIdQueryHandler(_headers, _profiles, _preferences);

            var response = await handler.Handle(
                new GetUserInfoByIdQueryRequest { username = "viewer", userId = "target-id" },
                CancellationToken.None);

            response.Body!.errorMessage.Should().NotBeNullOrEmpty();

            await _preferences.DidNotReceive().GetDescriptionsForViewerAsync(
                Arg.Any<string>(), Arg.Any<string>());
        }
    }
}
