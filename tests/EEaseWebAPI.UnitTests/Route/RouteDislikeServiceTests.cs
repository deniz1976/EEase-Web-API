using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.Features.Commands.Place.DislikePlace;
using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Contexts;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory.Infrastructure.Internal;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using UserNotFoundException = EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException;

namespace EEaseWebAPI.UnitTests.Route
{
    public class RouteDislikeServiceTests : IDisposable
    {
        private readonly EEaseAPIDbContext _context;

        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IPreferenceProfileBuilder _profileBuilder = Substitute.For<IPreferenceProfileBuilder>();
        private readonly IPreferenceFeedbackService _feedback = Substitute.For<IPreferenceFeedbackService>();
        private readonly IPlaceReplacementService _replacement = Substitute.For<IPlaceReplacementService>();
        private readonly IDislikedPlaceService _dislikedPlaces = Substitute.For<IDislikedPlaceService>();
        private readonly IRouteEnrichmentService _enrichment = Substitute.For<IRouteEnrichmentService>();
        private readonly RouteDislikeService _service;

        private readonly AppUser _alice = new() { Id = "alice-id", UserName = "alice" };
        private readonly AppUser _bob = new() { Id = "bob-id", UserName = "bob" };

        private readonly Guid _routeId = Guid.NewGuid();

        public RouteDislikeServiceTests()
        {
            _context = new EEaseAPIDbContext(
                new DbContextOptionsBuilder<EEaseAPIDbContext>()
                    .UseInMemoryDatabase($"dislike-{Guid.NewGuid():N}")
                    // The service runs inside a transaction; the in-memory provider has none
                    // and warns instead, which would fail the test rather than the code.
                    .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
                    .Options);

            _context.Users.AddRange(_alice, _bob);
            _context.StandardRoutes.Add(new StandardRoute
            {
                Id = _routeId,
                Name = "Rome",
                City = "Rome",
                UserId = _alice.Id,
                Days = 1,
                Status = 2,
                LikedUsers = new List<AppUser>(),
                TravelDays = new List<TravelDay>
                {
                    new()
                    {
                        Id = Guid.NewGuid(),
                        Dinner = new Dinner { Id = Guid.NewGuid(), GoogleId = "dinner-1" },
                        Accomodation = new TravelAccomodation { Id = Guid.NewGuid(), GoogleId = "hotel-1" }
                    }
                }
            });
            _context.SaveChanges();

            _userManager.FindByNameAsync("alice").Returns(_alice);
            _userManager.FindByNameAsync("bob").Returns(_bob);

            _feedback.ApplyAsync(Arg.Any<string>(), Arg.Any<BaseEntity>(), Arg.Any<string>(), Arg.Any<bool>())
                .Returns(new PreferenceFeedbackResult("food", new Dictionary<string, int> { ["Italian"] = -5 }));

            _profileBuilder.Build(
                    Arg.Any<UserAccommodationPreferences?>(),
                    Arg.Any<UserFoodPreferences?>(),
                    Arg.Any<UserPersonalization?>())
                .Returns(PreferenceProfile.Empty);

            _dislikedPlaces.GetGoogleIdsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Array.Empty<string>());

            _replacement.ReplaceAsync(
                    Arg.Any<StandardRoute>(), Arg.Any<PreferenceProfile>(), Arg.Any<string>(),
                    Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
                .Returns(new TravelDay());

            _service = new RouteDislikeService(
                _userManager, _context, _profileBuilder, _feedback, _replacement,
                _dislikedPlaces, _enrichment, NullLogger<RouteDislikeService>.Instance);
        }

        public void Dispose() => _context.Dispose();

        private DislikePlaceCommandRequest Request(
            string username = "alice",
            string? routeId = null,
            string googlePlaceId = "dinner-1",
            string placeType = "dinner") =>
            new()
            {
                Username = username,
                RouteId = routeId ?? _routeId.ToString(),
                GooglePlaceId = googlePlaceId,
                PlaceType = placeType
            };

        [Fact]
        public async Task A_dislike_pulls_the_matching_preferences_down()
        {
            await _service.DislikePlaceAsync(Request());

            await _feedback.Received(1).ApplyAsync(_alice.Id, Arg.Any<BaseEntity>(), "dinner", false);
        }

        [Fact]
        public async Task A_disliked_place_is_remembered_so_it_is_never_suggested_again()
        {
            await _service.DislikePlaceAsync(Request());

            await _dislikedPlaces.Received(1).RecordAsync(_alice.Id, "dinner-1", "dinner", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task The_disliked_place_is_swapped_for_another_one()
        {
            await _service.DislikePlaceAsync(Request());

            await _replacement.Received(1).ReplaceAsync(
                Arg.Any<StandardRoute>(), Arg.Any<PreferenceProfile>(), "dinner-1", "dinner",
                Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task The_route_is_enriched_again_after_the_swap()
        {
            await _service.DislikePlaceAsync(Request());

            await _enrichment.Received(1).ApplyAsync(
                Arg.Any<StandardRoute>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>());
        }

        [Fact]
        public async Task Every_place_the_user_disliked_before_is_kept_out_of_the_replacement()
        {
            _dislikedPlaces.GetGoogleIdsAsync(_alice.Id, Arg.Any<CancellationToken>())
                .Returns(new[] { "dinner-9", "dinner-8" });

            await _service.DislikePlaceAsync(Request());

            await _replacement.Received(1).ReplaceAsync(
                Arg.Any<StandardRoute>(), Arg.Any<PreferenceProfile>(), "dinner-1", "dinner",
                Arg.Is<IReadOnlyCollection<string>>(excluded => excluded.Contains("dinner-9") && excluded.Contains("dinner-8")),
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task The_returned_route_does_not_carry_its_owner_back_to_the_caller()
        {
            var route = await _service.DislikePlaceAsync(Request());

            route.User.Should().BeNull();
        }

        [Fact]
        public async Task A_place_type_that_was_not_given_is_rejected()
        {
            await _service.Invoking(service => service.DislikePlaceAsync(Request(placeType: "  ")))
                .Should().ThrowAsync<InvalidPlaceTypeException>();
        }

        [Fact]
        public async Task A_google_id_that_was_not_given_is_rejected()
        {
            await _service.Invoking(service => service.DislikePlaceAsync(Request(googlePlaceId: string.Empty)))
                .Should().ThrowAsync<InvalidPlaceTypeException>();
        }

        [Fact]
        public async Task A_route_id_that_is_not_a_guid_is_reported_as_a_missing_route()
        {
            await _service.Invoking(service => service.DislikePlaceAsync(Request(routeId: "not-a-guid")))
                .Should().ThrowAsync<RouteNotFoundException>();
        }

        [Fact]
        public async Task An_unknown_user_is_reported_as_such()
        {
            _userManager.FindByNameAsync("nobody").Returns((AppUser?)null);

            await _service.Invoking(service => service.DislikePlaceAsync(Request(username: "nobody")))
                .Should().ThrowAsync<UserNotFoundException>();
        }

        [Fact]
        public async Task Nobody_can_change_a_route_they_do_not_own()
        {
            await _service.Invoking(service => service.DislikePlaceAsync(Request(username: "bob")))
                .Should().ThrowAsync<ForbiddenException>();
        }

        [Fact]
        public async Task Disliking_a_place_that_is_not_part_of_the_route_is_rejected()
        {
            await _service.Invoking(service => service.DislikePlaceAsync(Request(googlePlaceId: "elsewhere-1")))
                .Should().ThrowAsync<PlaceNotFoundInRouteException>();
        }

        [Fact]
        public async Task Nothing_is_recorded_when_the_replacement_fails()
        {
            _replacement.ReplaceAsync(
                    Arg.Any<StandardRoute>(), Arg.Any<PreferenceProfile>(), Arg.Any<string>(),
                    Arg.Any<string>(), Arg.Any<IReadOnlyCollection<string>>(), Arg.Any<CancellationToken>())
                .Returns<TravelDay>(_ => throw new RouteGenerationException("nothing else nearby"));

            await _service.Invoking(service => service.DislikePlaceAsync(Request()))
                .Should().ThrowAsync<RouteGenerationException>();

            await _enrichment.DidNotReceive().ApplyAsync(
                Arg.Any<StandardRoute>(), Arg.Any<DateOnly?>(), Arg.Any<DateOnly?>());
        }
    }
}
