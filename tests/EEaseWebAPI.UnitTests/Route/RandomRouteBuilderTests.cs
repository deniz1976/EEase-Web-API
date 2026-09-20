using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using GooglePlace = EEaseWebAPI.Application.DTOs.GooglePlaces.Place;

namespace EEaseWebAPI.UnitTests.Route
{
    public class RandomRouteBuilderTests
    {
        private readonly IPlaceSearchService _search = Substitute.For<IPlaceSearchService>();
        private readonly IPlaceSelectionService _selection = Substitute.For<IPlaceSelectionService>();
        private readonly IPlaceQueryBuilder _queries = Substitute.For<IPlaceQueryBuilder>();
        private readonly IRouteEnrichmentService _enrichment = Substitute.For<IRouteEnrichmentService>();
        private readonly IRoutePlanValidator _validator = Substitute.For<IRoutePlanValidator>();
        private readonly ISystemUserProvider _systemUser = Substitute.For<ISystemUserProvider>();
        private readonly RandomRouteBuilder _builder;

        private static readonly DateOnly Start = new(2026, 5, 1);
        private static readonly DateOnly End = new(2026, 5, 2);

        public RandomRouteBuilderTests()
        {
            _systemUser.GetOrCreateAsync().Returns(new AppUser { Id = "system-id", UserName = "system" });

            _queries.HotelStars(Arg.Any<PRICE_LEVEL?>()).Returns("4");
            _queries.PricePrefix(Arg.Any<PRICE_LEVEL?>()).Returns("Moderate ");

            GivenSearchResults(20);
            GivenSelectionWorks();

            _validator.Validate(Arg.Any<StandardRoute>()).Returns(Valid);

            _builder = new RandomRouteBuilder(
                _search, _selection, _queries, _enrichment, _validator, _systemUser,
                NullLogger<RandomRouteBuilder>.Instance, new Random(1));
        }

        private static RoutePlanValidationResult Valid =>
            new(true, 0, 0, Array.Empty<string>(), Array.Empty<string>());

        private static RoutePlanValidationResult Invalid =>
            new(false, 16, 12, Array.Empty<string>(), new[] { "day 1 dinner" });

        private static IReadOnlyList<GooglePlace> Places(int count, string prefix) =>
            Enumerable.Range(1, count).Select(index => new GooglePlace { Id = $"{prefix}-{index}" }).ToList();

        private void GivenSearchResults(int count)
        {
            _search.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(call => Places(count, "search"));

            _search.SearchFirstMatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(call => Places(count, "first"));

            _search.CollectPlaceIdsAsync(
                    Arg.Any<IEnumerable<string>>(), Arg.Any<int>(),
                    Arg.Any<IEnumerable<string>?>(), Arg.Any<CancellationToken>())
                .Returns(call => Enumerable.Range(1, count).Select(index => $"collected-{index}").ToList());
        }

        private void GivenSelectionWorks()
        {
            _selection.MaterializeAsync<TravelAccomodation>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(_ => new TravelAccomodation { Id = Guid.NewGuid(), GoogleId = "hotel-1" });

            _selection.MaterializeAsync<Breakfast>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Breakfast { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<Lunch>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Lunch { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<Dinner>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Dinner { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<PlaceAfterDinner>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new PlaceAfterDinner { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<Place>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Place { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });
        }

        private Task<StandardRoute> Build(
            string destination = "rome",
            DateOnly? start = null,
            DateOnly? end = null,
            PRICE_LEVEL? priceLevel = PRICE_LEVEL.PRICE_LEVEL_MODERATE) =>
            _builder.BuildAsync(destination, start ?? Start, end ?? End, priceLevel);

        [Fact]
        public async Task A_route_covers_every_day_of_the_stay()
        {
            var route = await Build();

            route.Days.Should().Be(2);
            route.TravelDays.Should().HaveCount(2);
        }

        [Fact]
        public async Task Every_day_gets_a_hotel_three_sights_three_meals_and_an_evening()
        {
            var route = await Build();

            route.TravelDays.Should().OnlyContain(day =>
                day.Accomodation != null &&
                day.Breakfast != null && day.Lunch != null && day.Dinner != null &&
                day.PlaceAfterDinner != null &&
                day.FirstPlace != null && day.SecondPlace != null && day.ThirdPlace != null);
        }

        [Fact]
        public async Task Each_day_gets_its_own_copy_of_the_hotel()
        {
            var route = await Build();

            route.TravelDays.Select(day => day.Accomodation!.Id).Should().OnlyHaveUniqueItems();
            route.TravelDays.Select(day => day.Accomodation!.GoogleId).Should().AllBe("hotel-1");
        }

        [Fact]
        public async Task The_destination_is_written_the_same_way_however_it_arrives()
        {
            (await Build("  rOmE ")).City.Should().Be("Rome");
        }

        [Fact]
        public async Task A_random_route_belongs_to_the_system_user_and_is_published()
        {
            var route = await Build();

            route.UserId.Should().Be("system-id");
            route.Status.Should().Be(2);
        }

        [Fact]
        public async Task The_route_is_enriched_before_it_is_handed_back()
        {
            await Build();

            await _enrichment.Received(1).ApplyAsync(Arg.Any<StandardRoute>(), Start, End);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task A_destination_is_required(string? destination)
        {
            await _builder.Invoking(builder => builder.BuildAsync(destination!, Start, End, null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task Both_dates_are_required()
        {
            await _builder.Invoking(builder => builder.BuildAsync("Rome", null, End, null))
                .Should().ThrowAsync<ArgumentNullException>();

            await _builder.Invoking(builder => builder.BuildAsync("Rome", Start, null, null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task A_stay_cannot_end_before_it_starts()
        {
            await _builder.Invoking(builder => builder.BuildAsync("Rome", End, Start, null))
                .Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        public async Task A_city_with_no_hotel_cannot_be_planned()
        {
            _search.SearchFirstMatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(Array.Empty<GooglePlace>());

            await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>()
                .WithMessage("*hotel*");
        }

        [Fact]
        public async Task A_city_with_too_few_places_is_reported_instead_of_half_a_route()
        {
            GivenSearchResults(1);

            var thrown = await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>();

            thrown.Which.Message.Should().Contain("touristic");
        }

        [Fact]
        public async Task A_plan_that_never_validates_is_refused_after_three_attempts()
        {
            _validator.Validate(Arg.Any<StandardRoute>()).Returns(Invalid);

            await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>();

            _validator.Received(3).Validate(Arg.Any<StandardRoute>());
        }

        [Fact]
        public async Task A_plan_that_validates_on_the_second_try_is_kept()
        {
            _validator.Validate(Arg.Any<StandardRoute>()).Returns(Invalid, Valid);

            var route = await Build();

            route.TravelDays.Should().HaveCount(2);
            _validator.Received(2).Validate(Arg.Any<StandardRoute>());
        }

        [Fact]
        public async Task A_day_that_cannot_be_filled_is_retried_before_giving_up()
        {
            var attempts = 0;

            _selection.MaterializeAsync<Dinner>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => ++attempts == 1
                    ? throw new InvalidOperationException("no dinner left")
                    : Task.FromResult(new Dinner { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() }));

            var route = await Build();

            route.TravelDays.Should().OnlyContain(day => day.Dinner != null);
        }

        [Fact]
        public async Task A_city_where_no_day_can_ever_be_filled_is_reported()
        {
            _selection.MaterializeAsync<Dinner>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns<Dinner>(_ => throw new InvalidOperationException("no dinner left"));

            await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>();
        }

        [Fact]
        public async Task No_place_is_used_twice_anywhere_in_the_route()
        {
            var route = await Build();

            var used = route.TravelDays.SelectMany(day => new[]
            {
                day.Breakfast!.GoogleId, day.Lunch!.GoogleId, day.Dinner!.GoogleId,
                day.PlaceAfterDinner!.GoogleId,
                day.FirstPlace!.GoogleId, day.SecondPlace!.GoogleId, day.ThirdPlace!.GoogleId
            });

            used.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task The_searches_that_do_not_need_each_other_are_made_together()
        {
            var inFlight = 0;
            var highWater = 0;

            _search.CollectPlaceIdsAsync(
                    Arg.Any<IEnumerable<string>>(), Arg.Any<int>(),
                    Arg.Any<IEnumerable<string>?>(), Arg.Any<CancellationToken>())
                .Returns(async call =>
                {
                    highWater = Math.Max(highWater, Interlocked.Increment(ref inFlight));
                    await Task.Delay(20);
                    Interlocked.Decrement(ref inFlight);

                    return (IReadOnlyList<string>)Enumerable.Range(1, 20)
                        .Select(index => $"collected-{index}").ToList();
                });

            await Build();

            // Touristic places and evening venues are looked for at the same time; they used
            // to be waited for one after the other, and so did the hotel and the three meals.
            highWater.Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task The_builder_does_not_save_anything_itself()
        {
            var route = await Build();

            route.Id.Should().NotBeEmpty();
            route.User.Should().BeNull();
        }
    }
}
