using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PlaceSearchServiceTests
    {
        private readonly IGooglePlacesService _googlePlaces = Substitute.For<IGooglePlacesService>();
        private readonly PlaceSearchService _service;

        public PlaceSearchServiceTests()
        {
            _service = new PlaceSearchService(_googlePlaces, NullLogger<PlaceSearchService>.Instance);
        }

        private void Returns(string query, params Place[] places) =>
            _googlePlaces.SearchPlacesAsync(query)
                .Returns(new PlaceSearchResponse { Places = places.ToList() });

        private static Place PlaceWith(string? id, params string[] types) =>
            new() { Id = id, Types = types.Length == 0 ? null : types.ToList() };

        [Fact]
        public async Task Excluded_place_types_are_dropped()
        {
            Returns(
                "shops in Lisbon",
                PlaceWith("keep", "tourist_attraction"),
                PlaceWith("drop", "shopping_mall"),
                PlaceWith("drop-too", "restaurant", "supermarket"));

            var places = await _service.SearchAsync("shops in Lisbon");

            places.Select(place => place.Id).Should().Equal("keep");
        }

        [Fact]
        public async Task Places_without_an_id_are_dropped()
        {
            Returns("x", PlaceWith(null), PlaceWith("   "), PlaceWith("real"));

            var places = await _service.SearchAsync("x");

            places.Select(place => place.Id).Should().Equal("real");
        }

        [Fact]
        public async Task A_place_without_types_is_kept()
        {
            Returns("x", PlaceWith("untyped"));

            (await _service.SearchAsync("x")).Should().ContainSingle();
        }

        [Fact]
        public async Task An_empty_provider_response_yields_an_empty_list()
        {
            _googlePlaces.SearchPlacesAsync("nothing").Returns((PlaceSearchResponse?)null);

            (await _service.SearchAsync("nothing")).Should().BeEmpty();
            (await _service.SearchAsync("  ")).Should().BeEmpty();
        }

        [Fact]
        public async Task A_blank_query_is_never_sent_to_the_provider()
        {
            await _service.SearchAsync("   ");

            await _googlePlaces.DidNotReceive().SearchPlacesAsync(Arg.Any<string>());
        }

        [Fact]
        public async Task The_first_query_with_results_wins_and_the_rest_are_skipped()
        {
            _googlePlaces.SearchPlacesAsync("first").Returns(new PlaceSearchResponse { Places = new List<Place>() });
            Returns("second", PlaceWith("hit"));

            var places = await _service.SearchFirstMatchAsync(new[] { "first", "second", "third" });

            places.Select(place => place.Id).Should().Equal("hit");
            await _googlePlaces.DidNotReceive().SearchPlacesAsync("third");
        }

        [Fact]
        public async Task Collected_ids_are_distinct_across_queries()
        {
            Returns("a", PlaceWith("one"), PlaceWith("two"));
            Returns("b", PlaceWith("two"), PlaceWith("three"));

            var ids = await _service.CollectPlaceIdsAsync(new[] { "a", "b" });

            ids.Should().Equal("one", "two", "three");
        }

        [Fact]
        public async Task Excluded_ids_are_left_out()
        {
            Returns("a", PlaceWith("one"), PlaceWith("two"));

            var ids = await _service.CollectPlaceIdsAsync(new[] { "a" }, excludedPlaceIds: new[] { "one" });

            ids.Should().Equal("two");
        }

        [Fact]
        public async Task Collecting_stops_once_enough_ids_are_found()
        {
            Returns("a", PlaceWith("one"), PlaceWith("two"));
            Returns("b", PlaceWith("three"));

            var ids = await _service.CollectPlaceIdsAsync(new[] { "a", "b" }, requiredCount: 2);

            ids.Should().Equal("one", "two");
            await _googlePlaces.DidNotReceive().SearchPlacesAsync("b");
        }

        [Fact]
        public async Task Without_a_required_count_every_query_runs()
        {
            Returns("a", PlaceWith("one"));
            Returns("b", PlaceWith("two"));

            var ids = await _service.CollectPlaceIdsAsync(new[] { "a", "b" });

            ids.Should().Equal("one", "two");
            await _googlePlaces.Received(1).SearchPlacesAsync("b");
        }

        [Fact]
        public async Task Collecting_also_drops_excluded_place_types()
        {
            Returns("a", PlaceWith("keep", "museum"), PlaceWith("skip", "gas_station"));

            var ids = await _service.CollectPlaceIdsAsync(new[] { "a" });

            ids.Should().Equal("keep");
        }
    }
}
