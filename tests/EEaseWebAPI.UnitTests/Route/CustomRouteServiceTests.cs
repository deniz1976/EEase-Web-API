using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class CustomRouteServiceTests : IDisposable
    {
        private readonly IRandomRouteBuilder _randomBuilder = Substitute.For<IRandomRouteBuilder>();
        private readonly IPreferenceRouteBuilder _preferenceBuilder = Substitute.For<IPreferenceRouteBuilder>();
        private readonly EEaseAPIDbContext _context;
        private readonly CustomRouteService _service;

        public CustomRouteServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase(Guid.NewGuid().ToString())
                    .Options);

            _service = new CustomRouteService(_randomBuilder, _preferenceBuilder, _context);
        }

        public void Dispose() => _context.Dispose();

        private static StandardRoute RouteFor(string city) => new()
        {
            Id = Guid.NewGuid(),
            City = city,
            CreatedDate = DateTime.UtcNow,
            User = new AppUser { Id = "alice-id", UserName = "alice" },
            UserId = "alice-id"
        };

        [Fact]
        public async Task A_random_route_is_kept()
        {
            _randomBuilder
                .BuildAsync("Rome", null, null, null, Arg.Any<CancellationToken>())
                .Returns(RouteFor("Rome"));

            var route = await _service.CreateRandomRoute("Rome", null, null, null);

            route.City.Should().Be("Rome");
            (await _context.StandardRoutes.SingleAsync()).City.Should().Be("Rome");
        }

        [Fact]
        public async Task The_owner_is_left_out_of_the_answer_but_kept_on_the_row()
        {
            _randomBuilder
                .BuildAsync("Rome", null, null, null, Arg.Any<CancellationToken>())
                .Returns(RouteFor("Rome"));

            var route = await _service.CreateRandomRoute("Rome", null, null, null);

            route.User.Should().BeNull();
            route.UserId.Should().Be("alice-id");
        }

        [Fact]
        public async Task A_caller_who_gives_up_stops_the_building()
        {
            using var cancellation = new CancellationTokenSource();

            _preferenceBuilder
                .BuildAsync("Rome", null, null, null, "alice", null, Arg.Any<CancellationToken>())
                .Returns(RouteFor("Rome"));

            await _service.CreatePrefRoute(
                "Rome", null, null, null, "alice", null, cancellation.Token);

            await _preferenceBuilder.Received(1).BuildAsync(
                "Rome", null, null, null, "alice", null, cancellation.Token);
        }

        [Fact]
        public async Task A_preference_route_is_kept_too()
        {
            _preferenceBuilder
                .BuildAsync("Milan", null, null, null, "alice", null, Arg.Any<CancellationToken>())
                .Returns(RouteFor("Milan"));

            await _service.CreatePrefRoute("Milan", null, null, null, "alice", null);

            (await _context.StandardRoutes.SingleAsync()).City.Should().Be("Milan");
        }
    }
}
