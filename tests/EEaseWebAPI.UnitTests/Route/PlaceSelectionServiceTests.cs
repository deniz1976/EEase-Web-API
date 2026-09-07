using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PlacePickerTests
    {
        private static readonly string[] Pool = { "a", "b", "c" };

        [Fact]
        public void Take_never_hands_out_the_same_place_twice()
        {
            var picker = new PlacePicker(new Random(1));

            var taken = new[] { picker.Take(Pool), picker.Take(Pool), picker.Take(Pool) };

            taken.Should().OnlyHaveUniqueItems();
            taken.Should().BeEquivalentTo(Pool);
        }

        [Fact]
        public void Take_returns_null_once_the_pool_is_exhausted()
        {
            var picker = new PlacePicker(new Random(1));

            for (var index = 0; index < Pool.Length; index++)
            {
                picker.Take(Pool).Should().NotBeNull();
            }

            picker.Take(Pool).Should().BeNull();
        }

        [Fact]
        public void Blank_and_duplicate_entries_in_the_pool_are_ignored()
        {
            var picker = new PlacePicker(new Random(1));
            var pool = new[] { "a", "a", "  ", "b" };

            var first = picker.Take(pool);
            var second = picker.Take(pool);

            new[] { first, second }.Should().BeEquivalentTo(new[] { "a", "b" });
            picker.Take(pool).Should().BeNull();
        }

        [Fact]
        public void TakeAt_spreads_deterministically_and_still_never_repeats()
        {
            var picker = new PlacePicker(new Random(1));

            picker.TakeAt(Pool, 0).Should().Be("a");
            picker.TakeAt(Pool, 1).Should().Be("c");
            picker.TakeAt(Pool, 2).Should().Be("b");
            picker.TakeAt(Pool, 3).Should().BeNull();
        }

        [Fact]
        public void A_negative_offset_is_still_inside_the_pool()
        {
            var picker = new PlacePicker(new Random(1));

            picker.TakeAt(Pool, -1).Should().Be("c");
        }

        [Fact]
        public void Ids_marked_as_used_are_skipped()
        {
            var picker = new PlacePicker(new Random(1));

            picker.MarkUsed("a").Should().BeTrue();
            picker.MarkUsed("a").Should().BeFalse();
            picker.IsUsed("a").Should().BeTrue();

            new[] { picker.Take(Pool), picker.Take(Pool) }.Should().BeEquivalentTo(new[] { "b", "c" });
        }

        [Fact]
        public void Used_ids_are_visible_to_the_caller()
        {
            var picker = new PlacePicker(new Random(1));
            picker.Take(Pool);

            picker.UsedGoogleIds.Should().HaveCount(1);
        }
    }

    public class PlaceSelectionServiceTests
    {
        private readonly IGooglePlacesService _googlePlaces = Substitute.For<IGooglePlacesService>();
        private readonly PlaceSelectionService _service;

        public PlaceSelectionServiceTests()
        {
            _service = new PlaceSelectionService(_googlePlaces, NullLogger<PlaceSelectionService>.Instance);

            _googlePlaces.GetPlaceDetailsAsync(Arg.Any<string>())
                .Returns(call => $"{{\"displayName\": {{\"text\": \"Place {call.Arg<string>()}\"}}}}");
        }

        [Fact]
        public async Task A_selected_place_carries_its_id_google_id_and_price_level()
        {
            var picker = new PlacePicker(new Random(1));

            var breakfast = await _service.SelectAsync<Breakfast>(
                new[] { "abc" }, picker, PRICE_LEVEL.PRICE_LEVEL_MODERATE);

            breakfast.GoogleId.Should().Be("abc");
            breakfast.Id.Should().NotBe(Guid.Empty);
            breakfast._PRICE_LEVEL.Should().Be(PRICE_LEVEL.PRICE_LEVEL_MODERATE);
            breakfast.DisplayName!.Text.Should().Be("Place abc");
        }

        [Fact]
        public async Task Selections_share_one_picker_so_a_place_is_never_reused()
        {
            var picker = new PlacePicker(new Random(1));
            var pool = new[] { "a", "b" };

            var breakfast = await _service.SelectAsync<Breakfast>(pool, picker);
            var lunch = await _service.SelectAsync<Lunch>(pool, picker);

            breakfast.GoogleId.Should().NotBe(lunch.GoogleId);
        }

        [Fact]
        public async Task An_exhausted_pool_is_reported_instead_of_handing_back_a_duplicate()
        {
            var picker = new PlacePicker(new Random(1));

            await _service.SelectAsync<Breakfast>(new[] { "only" }, picker);

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SelectAsync<Lunch>(new[] { "only" }, picker));

            exception.Message.Should().Contain("Lunch");
        }

        [Fact]
        public async Task SelectMany_returns_the_requested_count_all_distinct()
        {
            var picker = new PlacePicker(new Random(1));

            var places = await _service.SelectManyAsync<Place>(new[] { "a", "b", "c", "d" }, picker, 3);

            places.Should().HaveCount(3);
            places.Select(place => place.GoogleId).Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task SelectMany_with_an_offset_gives_each_day_a_different_slice()
        {
            var pool = new[] { "a", "b", "c", "d", "e", "f" };
            var picker = new PlacePicker(new Random(1));

            var firstDay = await _service.SelectManyAsync<Place>(pool, picker, 3, offset: 0);
            var secondDay = await _service.SelectManyAsync<Place>(pool, picker, 3, offset: 3);

            firstDay.Select(place => place.GoogleId)
                .Should().NotIntersectWith(secondDay.Select(place => place.GoogleId));
        }

        [Fact]
        public async Task SelectMany_reports_a_pool_that_cannot_cover_the_request()
        {
            var picker = new PlacePicker(new Random(1));

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.SelectManyAsync<Place>(new[] { "a", "b" }, picker, 3));
        }

        [Fact]
        public async Task Unreadable_details_are_reported_with_the_place_id()
        {
            _googlePlaces.GetPlaceDetailsAsync("broken").Returns("<<not json>>");

            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => _service.MaterializeAsync<Place>("broken"));

            exception.Message.Should().Contain("broken");
        }

        [Fact]
        public async Task A_missing_place_id_is_refused_before_calling_the_provider()
        {
            await Assert.ThrowsAsync<InvalidOperationException>(() => _service.MaterializeAsync<Place>("  "));

            await _googlePlaces.DidNotReceive().GetPlaceDetailsAsync(Arg.Any<string>());
        }
    }
}
