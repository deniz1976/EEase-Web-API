using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Persistence.Services;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Routes
{
    public class RouteAccessPolicyTests
    {
        private const string OwnerId = "owner-id";
        private const string OwnerUserName = "owner";
        private const string RequesterId = "requester-id";
        private const string RequesterUserName = "requester";

        private readonly IUserService _userService = Substitute.For<IUserService>();

        private RouteAccessPolicy CreatePolicy() => new(_userService);

        private static StandardRoute CreateRoute(RouteVisibility visibility) => new()
        {
            Id = Guid.NewGuid(),
            UserId = OwnerId,
            User = new AppUser { Id = OwnerId, UserName = OwnerUserName },
            Status = (int)visibility,
            TravelDays = new List<TravelDay> { new() { DayDescription = "First day" } }
        };

        [Theory]
        [InlineData(RouteVisibility.Private)]
        [InlineData(RouteVisibility.FriendsOnly)]
        [InlineData(RouteVisibility.Public)]
        public async Task Owner_can_access_own_route_at_any_visibility(RouteVisibility visibility)
        {
            var route = CreateRoute(visibility);

            var result = await CreatePolicy().EvaluateAsync(route, OwnerUserName, OwnerId);

            result.IsAccessible.Should().BeTrue();
            result.Message.Should().BeNull();
        }

        [Fact]
        public async Task Private_route_is_closed_to_everyone_else()
        {
            var route = CreateRoute(RouteVisibility.Private);

            var result = await CreatePolicy().EvaluateAsync(route, RequesterUserName, RequesterId);

            result.IsAccessible.Should().BeFalse();
            result.Message.Should().Contain("private");
        }

        [Fact]
        public async Task Public_route_is_open_to_a_stranger()
        {
            var route = CreateRoute(RouteVisibility.Public);

            var result = await CreatePolicy().EvaluateAsync(route, RequesterUserName, RequesterId);

            result.IsAccessible.Should().BeTrue();
        }

        [Fact]
        public async Task Friends_only_route_is_open_to_a_friend()
        {
            var route = CreateRoute(RouteVisibility.FriendsOnly);
            _userService.IsFriendAsync(OwnerUserName, RequesterUserName).Returns(true);

            var result = await CreatePolicy().EvaluateAsync(route, RequesterUserName, RequesterId);

            result.IsAccessible.Should().BeTrue();
        }

        [Fact]
        public async Task Friends_only_route_is_closed_to_a_non_friend()
        {
            var route = CreateRoute(RouteVisibility.FriendsOnly);
            _userService.IsFriendAsync(OwnerUserName, RequesterUserName).Returns(false);

            var result = await CreatePolicy().EvaluateAsync(route, RequesterUserName, RequesterId);

            result.IsAccessible.Should().BeFalse();
            result.Message.Should().Contain("friends");
        }

        [Fact]
        public async Task Friends_only_route_is_denied_when_the_owner_is_not_loaded()
        {
            var route = CreateRoute(RouteVisibility.FriendsOnly);
            route.User = null;

            var result = await CreatePolicy().EvaluateAsync(route, RequesterUserName, RequesterId);

            result.IsAccessible.Should().BeFalse();
            await _userService.DidNotReceiveWithAnyArgs().IsFriendAsync(default!, default!);
        }

        [Theory]
        [InlineData(null)]
        [InlineData(7)]
        [InlineData(-1)]
        public async Task Unknown_visibility_value_is_denied(int? status)
        {
            var route = CreateRoute(RouteVisibility.Public);
            route.Status = status;

            var result = await CreatePolicy().EvaluateAsync(route, RequesterUserName, RequesterId);

            result.IsAccessible.Should().BeFalse();
            result.Message.Should().Contain("Invalid");
        }

        [Fact]
        public async Task Inaccessible_route_has_its_days_and_status_trimmed()
        {
            var route = CreateRoute(RouteVisibility.Private);
            route.City = "Rome";
            route.Name = "Rome tour";

            var dto = await CreatePolicy().ToDtoAsync(route, RequesterUserName, RequesterId);

            dto.City.Should().Be("Rome");
            dto.Name.Should().Be("Rome tour");
            dto.IsAccessible.Should().BeFalse();
            dto.AccessibilityMessage.Should().NotBeNullOrWhiteSpace();
            dto.TravelDays.Should().BeNull();
            dto.Status.Should().BeNull();
        }

        [Fact]
        public async Task Accessible_route_returns_its_day_details()
        {
            var route = CreateRoute(RouteVisibility.Public);

            var dto = await CreatePolicy().ToDtoAsync(route, RequesterUserName, RequesterId);

            dto.IsAccessible.Should().BeTrue();
            dto.TravelDays.Should().HaveCount(1);
            dto.Status.Should().Be((int)RouteVisibility.Public);
        }
    }
}
