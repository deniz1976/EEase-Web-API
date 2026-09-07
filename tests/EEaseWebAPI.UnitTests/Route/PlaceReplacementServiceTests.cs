using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PlaceReplacementServiceTests
    {
        private readonly IPlaceSearchService _search = Substitute.For<IPlaceSearchService>();
        private readonly IPlaceSelectionService _selection = Substitute.For<IPlaceSelectionService>();
        private readonly PlaceReplacementService _service;

        public PlaceReplacementServiceTests()
        {
            _service = new PlaceReplacementService(
                _search,
                _selection,
                new PlaceQueryBuilder(new Random(1)),
                NullLogger<PlaceReplacementService>.Instance,
                new Random(1));

            _selection.MaterializeAsync<Dinner>(Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Dinner { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<Domain.Entities.Route.Place>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Domain.Entities.Route.Place { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<TravelAccomodation>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new TravelAccomodation { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });
        }

        private void SearchFinds(params string[] googleIds) =>
            _search.CollectPlaceIdsAsync(
                    Arg.Any<IEnumerable<string>>(), Arg.Any<int>(),
                    Arg.Any<IEnumerable<string>?>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    var excluded = new HashSet<string>(
                        (IEnumerable<string>?)call[2] ?? Enumerable.Empty<string>());

                    return (IReadOnlyList<string>)googleIds.Where(id => !excluded.Contains(id)).ToList();
                });

        private static StandardRoute Route() => new()
        {
            Id = Guid.NewGuid(),
            City = "Lisbon",
            TravelDays = new List<TravelDay>
            {
                new()
                {
                    Accomodation = new TravelAccomodation
                        { GoogleId = "hotel-1", _PRICE_LEVEL = PRICE_LEVEL.PRICE_LEVEL_MODERATE },
                    Dinner = new Dinner { GoogleId = "dinner-1", Weather = new Weather { Degree = 21 } },
                    FirstPlace = new Domain.Entities.Route.Place { GoogleId = "place-1" },
                    SecondPlace = new Domain.Entities.Route.Place { GoogleId = "place-2" },
                    ThirdPlace = new Domain.Entities.Route.Place { GoogleId = "place-3" }
                },
                new()
                {
                    Accomodation = new TravelAccomodation
                        { GoogleId = "hotel-1", _PRICE_LEVEL = PRICE_LEVEL.PRICE_LEVEL_MODERATE },
                    Dinner = new Dinner { GoogleId = "dinner-2" },
                    FirstPlace = new Domain.Entities.Route.Place { GoogleId = "place-4" },
                    SecondPlace = new Domain.Entities.Route.Place { GoogleId = "place-5" },
                    ThirdPlace = new Domain.Entities.Route.Place { GoogleId = "place-6" }
                }
            }
        };

        private Task<TravelDay> Replace(
            StandardRoute route, string googleId, string placeType, params string[] disliked) =>
            _service.ReplaceAsync(route, PreferenceProfile.Empty, googleId, placeType, disliked);

        [Fact]
        public async Task A_place_that_is_not_in_the_route_is_reported()
        {
            SearchFinds("dinner-9");

            await Assert.ThrowsAsync<PlaceNotFoundInRouteException>(
                () => Replace(Route(), "dinner-404", "dinner"));
        }

        [Fact]
        public async Task An_unknown_place_type_is_reported()
        {
            await Assert.ThrowsAsync<InvalidPlaceTypeException>(
                () => Replace(Route(), "dinner-1", "spaceship"));
        }

        [Fact]
        public async Task The_disliked_place_is_never_offered_back()
        {
            var route = Route();
            SearchFinds("dinner-1");

            await Assert.ThrowsAsync<RouteGenerationException>(
                () => Replace(route, "dinner-1", "dinner"));

            route.TravelDays[0].Dinner!.GoogleId.Should().Be("dinner-1");
        }

        [Fact]
        public async Task A_place_already_used_elsewhere_in_the_route_is_not_offered()
        {
            SearchFinds("dinner-2", "place-5", "hotel-1");

            await Assert.ThrowsAsync<RouteGenerationException>(
                () => Replace(Route(), "dinner-1", "dinner"));
        }

        [Fact]
        public async Task A_place_the_user_disliked_before_is_not_offered()
        {
            SearchFinds("dinner-7");

            await Assert.ThrowsAsync<RouteGenerationException>(
                () => Replace(Route(), "dinner-1", "dinner", "dinner-7"));
        }

        [Fact]
        public async Task A_free_alternative_takes_the_slot()
        {
            var route = Route();
            SearchFinds("dinner-7");

            var day = await Replace(route, "dinner-1", "dinner");

            day.Should().BeSameAs(route.TravelDays[0]);
            route.TravelDays[0].Dinner!.GoogleId.Should().Be("dinner-7");
            route.TravelDays[1].Dinner!.GoogleId.Should().Be("dinner-2");
        }

        [Fact]
        public async Task The_weather_that_was_planned_for_the_slot_is_kept()
        {
            var route = Route();
            SearchFinds("dinner-7");

            await Replace(route, "dinner-1", "dinner");

            route.TravelDays[0].Dinner!.Weather!.Degree.Should().Be(21);
        }

        [Fact]
        public async Task A_touristic_slot_is_replaced_in_place()
        {
            var route = Route();
            SearchFinds("place-9");

            await Replace(route, "place-2", "place");

            route.TravelDays[0].SecondPlace!.GoogleId.Should().Be("place-9");
            route.TravelDays[0].FirstPlace!.GoogleId.Should().Be("place-1");
            route.TravelDays[0].ThirdPlace!.GoogleId.Should().Be("place-3");
        }

        [Fact]
        public async Task A_touristic_slot_on_a_later_day_is_found()
        {
            var route = Route();
            SearchFinds("place-9");

            var day = await Replace(route, "place-5", "place");

            day.Should().BeSameAs(route.TravelDays[1]);
            route.TravelDays[1].SecondPlace!.GoogleId.Should().Be("place-9");
        }

        [Fact]
        public async Task A_new_hotel_is_taken_for_every_day_of_the_route()
        {
            var route = Route();
            SearchFinds("hotel-9");

            await Replace(route, "hotel-1", "accommodation");

            route.TravelDays.Should().OnlyContain(day => day.Accomodation!.GoogleId == "hotel-9");
            route.TravelDays[0].Accomodation!.Id.Should().NotBe(route.TravelDays[1].Accomodation!.Id);
        }

        [Fact]
        public async Task The_search_widens_across_several_queries()
        {
            IEnumerable<string>? queries = null;

            _search.CollectPlaceIdsAsync(
                    Arg.Any<IEnumerable<string>>(), Arg.Any<int>(),
                    Arg.Any<IEnumerable<string>?>(), Arg.Any<CancellationToken>())
                .Returns(call =>
                {
                    queries = (IEnumerable<string>)call[0];
                    return (IReadOnlyList<string>)new List<string> { "dinner-7" };
                });

            await Replace(Route(), "dinner-1", "dinner");

            queries!.Should().HaveCountGreaterThan(1);
            queries.Should().OnlyContain(query => query.EndsWith("in Lisbon"));
        }
    }
}
