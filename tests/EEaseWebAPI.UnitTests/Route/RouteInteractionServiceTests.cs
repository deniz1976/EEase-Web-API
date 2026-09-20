using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using UserNotFoundException = EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException;
using EEaseWebAPI.Application;
using EEaseWebAPI.UnitTests.Localization;

namespace EEaseWebAPI.UnitTests.Route
{
    public class RouteInteractionServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;

        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IRouteAccessPolicy _accessPolicy = Substitute.For<IRouteAccessPolicy>();
        private readonly IPreferenceFeedbackService _feedback = Substitute.For<IPreferenceFeedbackService>();
        private readonly RouteInteractionService _service;

        private readonly AppUser _alice = new() { Id = "alice-id", UserName = "alice" };
        private readonly AppUser _bob = new() { Id = "bob-id", UserName = "bob" };

        private readonly Guid _routeId = Guid.NewGuid();

        public RouteInteractionServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"interaction-{Guid.NewGuid():N}")
                    .Options);

            _context.Users.AddRange(_alice, _bob);
            _context.StandardRoutes.Add(new StandardRoute
            {
                Id = _routeId,
                Name = "Rome",
                City = "Rome",
                UserId = _alice.Id,
                Days = 1,
                LikeCount = 0,
                Status = 2,
                LikedUsers = new List<AppUser>(),
                TravelDays = new List<TravelDay>()
            });
            _context.SaveChanges();

            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.FindByNameAsync("bob").Returns(_bob);

            _accessPolicy.EvaluateAsync(Arg.Any<StandardRoute>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(RouteAccessResult.Allowed);

            _feedback.ApplyAsync(Arg.Any<string>(), Arg.Any<BaseEntity>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(new PreferenceFeedbackResult("accommodation", new Dictionary<string, int> { ["Luxury"] = 5 }));

            _service = new RouteInteractionService(
                _userManager, _context, _accessPolicy, _feedback, Localizers.For<AppMessages>());
        }

        public void Dispose() => _context.Dispose();

        private StandardRoute Route() => _context.StandardRoutes.Include(r => r.LikedUsers).First(r => r.Id == _routeId);

        [Fact]
        public async Task Liking_a_route_adds_the_user_and_counts_them()
        {
            (await _service.LikeRoute("alice", _routeId)).Should().BeTrue();

            var route = Route();
            route.LikedUsers.Should().ContainSingle();
            route.LikeCount.Should().Be(1);
        }

        [Fact]
        public async Task Liking_twice_is_an_unlike_and_the_count_follows()
        {
            await _service.LikeRoute("alice", _routeId);

            (await _service.LikeRoute("alice", _routeId)).Should().BeFalse();

            var route = Route();
            route.LikedUsers.Should().BeEmpty();
            route.LikeCount.Should().Be(0);
        }

        [Fact]
        public async Task An_unlike_never_drives_the_count_below_zero()
        {
            var route = Route();
            route.LikedUsers!.Add(_alice);
            route.LikeCount = 0;
            await _context.SaveChangesAsync();

            await _service.LikeRoute("alice", _routeId);

            Route().LikeCount.Should().Be(0);
        }

        [Fact]
        public async Task A_route_nobody_may_see_cannot_be_liked()
        {
            _accessPolicy.EvaluateAsync(Arg.Any<StandardRoute>(), "bob", Arg.Any<string>())
                .Returns(RouteAccessResult.Denied("private route"));

            var thrown = await _service.Invoking(service => service.LikeRoute("bob", _routeId))
                .Should().ThrowAsync<ForbiddenException>();

            thrown.Which.EnumStatusCode.Should().Be((int)StatusEnum.UnauthorizedToViewRoute);
        }

        [Fact]
        public async Task An_unknown_user_is_reported_as_such()
        {
            _userManager.FindByNameAsync("nobody").Returns((AppUser?)null);

            await _service.Invoking(service => service.LikeRoute("nobody", _routeId))
                .Should().ThrowAsync<UserNotFoundException>();
        }

        [Fact]
        public async Task A_missing_route_is_reported_as_such()
        {
            await _service.Invoking(service => service.LikeRoute("alice", Guid.NewGuid()))
                .Should().ThrowAsync<RouteNotFoundException>();
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public async Task The_owner_can_move_a_route_to_any_known_visibility(int status)
        {
            var response = await _service.UpdateRouteStatusAsync(_routeId, status, "alice");

            response.IsUpdated.Should().BeTrue();
            Route().Status.Should().Be(status);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(3)]
        public async Task A_visibility_that_does_not_exist_is_rejected(int status)
        {
            await _service.Invoking(service => service.UpdateRouteStatusAsync(_routeId, status, "alice"))
                .Should().ThrowAsync<InvalidRouteStatusException>();
        }

        [Fact]
        public async Task Only_the_owner_may_change_visibility()
        {
            await _service.Invoking(service => service.UpdateRouteStatusAsync(_routeId, 0, "bob"))
                .Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task Only_the_owner_may_delete_a_route()
        {
            await _service.Invoking(service => service.DeleteRoute("bob", _routeId))
                .Should().ThrowAsync<DeleteRouteException>();
        }

        [Fact]
        public async Task Deleting_all_routes_of_a_user_without_any_says_so()
        {
            var message = await Culture.UseAsync("en", () => _service.DeleteAllRoutes("bob"));

            message.Should().Be("route count is 0.");
        }

        [Fact]
        public async Task Deleting_all_routes_removes_them()
        {
            var message = await Culture.UseAsync("en", () => _service.DeleteAllRoutes("alice"));

            message.Should().Be("routes deleted successfully.");

            _context.StandardRoutes.Should().BeEmpty();
        }

        [Theory]
        [InlineData("accommodation")]
        [InlineData("Accommodation")]
        [InlineData("ACCOMMODATION")]
        public async Task A_place_type_is_matched_whatever_case_it_arrives_in(string placeType)
        {
            _context.TravelAccomodations.Add(new TravelAccomodation
            {
                Id = Guid.NewGuid(),
                GoogleId = "hotel-1"
            });
            await _context.SaveChangesAsync();

            var response = await _service.LikePlaceOrRestaurantAsync("alice", "hotel-1", placeType);

            response.IsPreferenceUpdated.Should().BeTrue();
        }

        [Fact]
        public async Task A_place_type_the_system_does_not_know_is_rejected()
        {
            await _service.Invoking(service => service.LikePlaceOrRestaurantAsync("alice", "hotel-1", "brunch"))
                .Should().ThrowAsync<InvalidPlaceTypeException>();
        }

        [Fact]
        public async Task Liking_a_place_that_was_never_stored_is_rejected()
        {
            await _service.Invoking(service => service.LikePlaceOrRestaurantAsync("alice", "unknown", "accommodation"))
                .Should().ThrowAsync<InvalidPlaceTypeException>();
        }
    }
}
