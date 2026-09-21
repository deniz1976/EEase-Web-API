using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;
using UserNotFoundException = EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException;

namespace EEaseWebAPI.UnitTests.Route
{
    public class RouteQueryServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;

        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IRouteAccessPolicy _accessPolicy = Substitute.For<IRouteAccessPolicy>();
        private readonly IFriendshipService _friendships = Substitute.For<IFriendshipService>();
        private readonly RouteQueryService _service;

        private readonly AppUser _alice = new() { Id = "alice-id", UserName = "alice" };
        private readonly AppUser _bob = new() { Id = "bob-id", UserName = "bob" };

        private readonly Guid _publicRouteId = Guid.NewGuid();
        private readonly Guid _friendsOnlyRouteId = Guid.NewGuid();
        private readonly Guid _privateRouteId = Guid.NewGuid();

        public RouteQueryServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"route-queries-{Guid.NewGuid():N}")
                    .Options);

            _context.Users.AddRange(_alice, _bob);
            _context.StandardRoutes.AddRange(
                NewRoute(_publicRouteId, RouteVisibility.Public),
                NewRoute(_friendsOnlyRouteId, RouteVisibility.FriendsOnly),
                NewRoute(_privateRouteId, RouteVisibility.Private));
            _context.SaveChanges();

            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.FindByNameAsync("bob").Returns(_bob);
            _userManager.FindByIdAsync("alice-id").Returns(_alice);

            _accessPolicy.EvaluateAsync(Arg.Any<StandardRoute>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(RouteAccessResult.Allowed);
            _accessPolicy.ToDtoAsync(Arg.Any<StandardRoute>(), Arg.Any<string>(), Arg.Any<string>())
                .Returns(call => new StandardRouteDTO { Id = call.Arg<StandardRoute>().Id });

            _service = new RouteQueryService(_userManager, _context, _accessPolicy, _friendships);
        }

        public void Dispose() => _context.Dispose();

        private StandardRoute NewRoute(Guid id, RouteVisibility visibility) => new()
        {
            Id = id,
            Name = "Rome",
            City = "Rome",
            UserId = _alice.Id,
            Days = 1,
            LikeCount = 0,
            Status = (int)visibility,
            LikedUsers = new List<AppUser>(),
            TravelDays = new List<TravelDay>()
        };

        [Fact]
        public async Task A_user_sees_every_route_they_own()
        {
            var (routes, totalCount) = await _service.GetAllRoutes("alice");

            totalCount.Should().Be(3);
            routes.Should().HaveCount(3);
        }

        [Fact]
        public async Task Paging_reports_the_full_count_but_returns_one_page()
        {
            var (routes, totalCount) = await _service.GetAllRoutes("alice", pageNumber: 1, pageSize: 2);

            totalCount.Should().Be(3);
            routes.Should().HaveCount(2);
        }

        [Fact]
        public async Task A_stranger_only_sees_the_public_routes_of_a_profile()
        {
            _friendships.AreFriendsAsync("alice", "bob").Returns(false);

            var (routes, totalCount) = await _service.GetRoutesByUserId("alice-id", "bob");

            totalCount.Should().Be(1);
            routes.Single().Id.Should().Be(_publicRouteId);
        }

        [Fact]
        public async Task A_friend_also_sees_the_friends_only_routes()
        {
            _friendships.AreFriendsAsync("alice", "bob").Returns(true);

            var (routes, _) = await _service.GetRoutesByUserId("alice-id", "bob");

            routes.Select(route => route.Id).Should().BeEquivalentTo(new[] { _publicRouteId, _friendsOnlyRouteId });
        }

        [Fact]
        public async Task On_their_own_profile_a_user_sees_their_private_routes_too()
        {
            var (routes, totalCount) = await _service.GetRoutesByUserId("alice-id", "alice");

            totalCount.Should().Be(3);
            routes.Should().HaveCount(3);
        }

        [Fact]
        public async Task A_route_that_does_not_exist_is_reported_as_missing()
        {
            await _service.Invoking(service => service.GetRouteById("alice", Guid.NewGuid()))
                .Should().ThrowAsync<RouteNotFoundException>();
        }

        [Fact]
        public async Task An_unknown_user_is_reported_as_such()
        {
            _userManager.FindByNameAsync("nobody").Returns((AppUser?)null);

            await _service.Invoking(service => service.GetRouteById("nobody", _publicRouteId))
                .Should().ThrowAsync<UserNotFoundException>();
        }

        [Fact]
        public async Task The_like_status_of_a_route_nobody_liked_is_false()
        {
            var (isLiked, likeCount) = await _service.CheckRouteLikeStatus("alice", _publicRouteId);

            isLiked.Should().BeFalse();
            likeCount.Should().Be(0);
        }

        [Fact]
        public async Task The_like_status_reflects_a_like()
        {
            var route = _context.StandardRoutes.Include(r => r.LikedUsers).First(r => r.Id == _publicRouteId);
            route.LikedUsers!.Add(_alice);
            await _context.SaveChangesAsync();

            var (isLiked, likeCount) = await _service.CheckRouteLikeStatus("alice", _publicRouteId);

            isLiked.Should().BeTrue();
            likeCount.Should().Be(1);
        }

        [Fact]
        public async Task Asking_for_the_like_status_of_a_missing_route_keeps_its_own_status_code()
        {
            var thrown = await _service.Invoking(service => service.CheckRouteLikeStatus("alice", Guid.NewGuid()))
                .Should().ThrowAsync<RouteNotFoundException>();

            thrown.Which.EnumStatusCode.Should().Be((int)StatusEnum.RouteNotFound);
        }

        [Fact]
        public async Task A_denied_like_status_says_it_was_denied_rather_than_unknown()
        {
            _accessPolicy.EvaluateAsync(Arg.Any<StandardRoute>(), "bob", Arg.Any<string>())
                .Returns(RouteAccessResult.Denied("private route"));

            var thrown = await _service.Invoking(service => service.CheckRouteLikeStatus("bob", _privateRouteId))
                .Should().ThrowAsync<ForbiddenException>();

            thrown.Which.EnumStatusCode.Should().Be((int)StatusEnum.UnauthorizedToViewRoute);
        }
    }
}
