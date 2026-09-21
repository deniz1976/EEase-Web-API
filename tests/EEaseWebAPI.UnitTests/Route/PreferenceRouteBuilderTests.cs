using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;
using GooglePlace = EEaseWebAPI.Application.DTOs.GooglePlaces.Place;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PreferenceRouteBuilderTests
    {
        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly IPlaceSearchService _search = Substitute.For<IPlaceSearchService>();
        private readonly IPlaceSelectionService _selection = Substitute.For<IPlaceSelectionService>();
        private readonly IPlaceQueryBuilder _queries = Substitute.For<IPlaceQueryBuilder>();
        private readonly IPreferenceProfileBuilder _profileBuilder = Substitute.For<IPreferenceProfileBuilder>();
        private readonly ITravellerPreferenceCollector _travellers = Substitute.For<ITravellerPreferenceCollector>();
        private readonly IRouteEnrichmentService _enrichment = Substitute.For<IRouteEnrichmentService>();
        private readonly IRoutePlanValidator _validator = Substitute.For<IRoutePlanValidator>();
        private readonly IDislikedPlaceService _dislikedPlaces = Substitute.For<IDislikedPlaceService>();
        private readonly PreferenceRouteBuilder _builder;

        private readonly AppUser _alice = new() { Id = "alice-id", UserName = "alice" };

        private static readonly DateOnly Start = new(2026, 5, 1);
        private static readonly DateOnly End = new(2026, 5, 2);

        public PreferenceRouteBuilderTests()
        {
            _userManager.FindByNameAsync("alice").Returns(_alice);

            _queries.HotelStars(Arg.Any<PRICE_LEVEL?>()).Returns("4");
            _queries.PricePrefix(Arg.Any<PRICE_LEVEL?>()).Returns("Moderate ");
            _queries.Accommodation(Arg.Any<IReadOnlyList<PreferenceItem>?>(), Arg.Any<PRICE_LEVEL?>())
                .Returns("4 star boutique hotel");
            _queries.Food(Arg.Any<IReadOnlyList<PreferenceItem>?>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<MealType>())
                .Returns("Moderate Italian restaurant");
            _queries.Touristic(Arg.Any<IReadOnlyList<PreferenceItem>?>()).Returns("Museums");
            _queries.AfterDinner(Arg.Any<IReadOnlyList<PreferenceItem>?>(), Arg.Any<PRICE_LEVEL?>()).Returns("Wine bars");
            _queries.AlternativeTouristic(Arg.Any<IReadOnlyList<PreferenceItem>?>(), Arg.Any<string>()).Returns("Galleries");
            _queries.AlternativeAfterDinner(Arg.Any<PRICE_LEVEL?>()).Returns("Cafes");
            _queries.AlternativeFood(Arg.Any<MealType>(), Arg.Any<PRICE_LEVEL?>()).Returns("Any restaurant");
            _queries.SelectPreference(Arg.Any<IReadOnlyList<PreferenceItem>?>()).Returns("BoutiqueHotelPreference");

            _travellers.CollectAsync(Arg.Any<AppUser>(), Arg.Any<IReadOnlyList<string>?>(), Arg.Any<CancellationToken>())
                .Returns(Array.Empty<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>());

            _profileBuilder.Build(Arg.Any<IEnumerable<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>>())
                .Returns(PreferenceProfile.Empty);

            _dislikedPlaces.GetGoogleIdsAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(Array.Empty<string>());

            GivenSearchResults(30);
            GivenSelectionWorks();

            _validator.Validate(Arg.Any<StandardRoute>())
                .Returns(new RoutePlanValidationResult(true, 0, 0, Array.Empty<string>(), Array.Empty<string>()));

            _builder = new PreferenceRouteBuilder(
                _userManager, _search, _selection, _queries, _profileBuilder, _travellers,
                _enrichment, _validator, _dislikedPlaces,
                NullLogger<PreferenceRouteBuilder>.Instance, new Random(1));
        }

        private static IReadOnlyList<GooglePlace> Places(int count, string prefix) =>
            Enumerable.Range(1, count).Select(index => new GooglePlace { Id = $"{prefix}-{index}" }).ToList();

        private void GivenSearchResults(int count)
        {
            _search.SearchAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(_ => Places(count, "search"));

            _search.SearchFirstMatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(_ => Places(count, "first"));

            _search.CollectPlaceIdsAsync(
                    Arg.Any<IEnumerable<string>>(), Arg.Any<int>(),
                    Arg.Any<IEnumerable<string>?>(), Arg.Any<CancellationToken>())
                .Returns(_ => Enumerable.Range(1, count).Select(index => $"collected-{index}").ToList());
        }

        private void GivenSelectionWorks()
        {
            _selection.MaterializeAsync<TravelAccomodation>(
                    Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(_ => new TravelAccomodation { Id = Guid.NewGuid(), GoogleId = "hotel-1" });

            _selection.MaterializeAsync<Breakfast>(Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Breakfast { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<Lunch>(Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Lunch { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.MaterializeAsync<Dinner>(Arg.Any<string>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<CancellationToken>())
                .Returns(call => new Dinner { Id = Guid.NewGuid(), GoogleId = call.Arg<string>() });

            _selection.SelectAsync<PlaceAfterDinner>(Arg.Any<IReadOnlyList<string>>(), Arg.Any<PlacePicker>(),
                    Arg.Any<PRICE_LEVEL?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
                .Returns(_ => new PlaceAfterDinner { Id = Guid.NewGuid() });

            _selection.SelectManyAsync<Place>(Arg.Any<IReadOnlyList<string>>(), Arg.Any<PlacePicker>(),
                    Arg.Any<int>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
                .Returns(_ => (IReadOnlyList<Place>)new List<Place>
                {
                    new() { Id = Guid.NewGuid() },
                    new() { Id = Guid.NewGuid() },
                    new() { Id = Guid.NewGuid() }
                });
        }

        private Task<StandardRoute> Build(
            string? destination = "rome",
            PRICE_LEVEL? priceLevel = PRICE_LEVEL.PRICE_LEVEL_MODERATE,
            string? username = "alice",
            List<string>? friends = null) =>
            _builder.BuildAsync(destination, Start, End, priceLevel, username, friends);

        [Fact]
        public async Task A_route_covers_every_day_and_fills_every_slot()
        {
            var route = await Build();

            route.TravelDays.Should().HaveCount(2);
            route.TravelDays.Should().OnlyContain(day =>
                day.Accomodation != null &&
                day.Breakfast != null && day.Lunch != null && day.Dinner != null &&
                day.PlaceAfterDinner != null &&
                day.FirstPlace != null && day.SecondPlace != null && day.ThirdPlace != null);
        }

        [Fact]
        public async Task The_route_belongs_to_the_user_who_asked_for_it()
        {
            (await Build()).UserId.Should().Be("alice-id");
        }

        [Fact]
        public async Task The_preferences_of_everyone_travelling_shape_the_route()
        {
            await Build(friends: new List<string> { "bob" });

            await _travellers.Received(1).CollectAsync(
                _alice,
                Arg.Is<IReadOnlyList<string>?>(friends => friends != null && friends.Contains("bob")),
                Arg.Any<CancellationToken>());

            _profileBuilder.Received(1).Build(
                Arg.Any<IEnumerable<(UserAccommodationPreferences?, UserFoodPreferences?, UserPersonalization?)>>());
        }

        [Fact]
        public async Task A_missing_price_level_falls_back_to_moderate()
        {
            await Build(priceLevel: null);

            _queries.Received().Accommodation(
                Arg.Any<IReadOnlyList<PreferenceItem>?>(), PRICE_LEVEL.PRICE_LEVEL_MODERATE);
        }

        [Fact]
        public async Task A_place_the_user_disliked_is_never_offered_as_the_hotel()
        {
            _search.SearchFirstMatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(new List<GooglePlace> { new() { Id = "hated-hotel" } });

            _dislikedPlaces.GetGoogleIdsAsync("alice-id", Arg.Any<CancellationToken>())
                .Returns(new[] { "hated-hotel" });

            await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>()
                .WithMessage("*hotel*");
        }

        [Fact]
        public async Task The_hotel_records_which_preference_picked_it()
        {
            var route = await Build();

            route.TravelDays[0].Accomodation!.UserAccomodationPreference.Should().NotBeNullOrEmpty();
        }

        [Fact]
        public async Task Each_day_gets_its_own_copy_of_the_hotel()
        {
            var route = await Build();

            route.TravelDays.Select(day => day.Accomodation!.Id).Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task The_route_is_enriched_before_it_is_handed_back()
        {
            await Build();

            await _enrichment.Received(1).ApplyAsync(Arg.Any<StandardRoute>(), Start, End);
        }

        [Fact]
        public async Task An_unknown_user_cannot_get_a_route()
        {
            _userManager.FindByNameAsync("nobody").Returns((AppUser?)null);

            await _builder.Invoking(builder => Build(username: "nobody"))
                .Should().ThrowAsync<UserNotFoundException>();
        }

        [Fact]
        public async Task A_username_is_required()
        {
            await _builder.Invoking(builder => Build(username: "   "))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task A_destination_is_required()
        {
            await _builder.Invoking(builder => Build(destination: null))
                .Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task A_city_with_no_restaurant_at_all_is_reported()
        {
            _search.SearchFirstMatchAsync(Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>())
                .Returns(
                    Places(3, "hotel"),
                    Array.Empty<GooglePlace>());

            await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>()
                .WithMessage("*Breakfast*");
        }

        [Fact]
        public async Task A_day_that_cannot_be_filled_is_retried_with_a_wider_search()
        {
            var attempts = 0;

            _selection.SelectManyAsync<Place>(Arg.Any<IReadOnlyList<string>>(), Arg.Any<PlacePicker>(),
                    Arg.Any<int>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
                .Returns(_ => ++attempts == 1
                    ? throw new InvalidOperationException("nothing unique left")
                    : Task.FromResult<IReadOnlyList<Place>>(new List<Place>
                    {
                        new() { Id = Guid.NewGuid() },
                        new() { Id = Guid.NewGuid() },
                        new() { Id = Guid.NewGuid() }
                    }));

            var route = await Build();

            route.TravelDays.Should().OnlyContain(day => day.FirstPlace != null);
        }

        [Fact]
        public async Task A_day_that_can_never_be_filled_is_reported_after_the_retries()
        {
            _selection.SelectManyAsync<Place>(Arg.Any<IReadOnlyList<string>>(), Arg.Any<PlacePicker>(),
                    Arg.Any<int>(), Arg.Any<PRICE_LEVEL?>(), Arg.Any<int?>(), Arg.Any<CancellationToken>())
                .Returns<IReadOnlyList<Place>>(_ => throw new InvalidOperationException("nothing unique left"));

            await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>()
                .WithMessage("*Day 1*");
        }

        [Fact]
        public async Task A_meal_is_searched_for_once_however_long_the_trip_is()
        {
            await _builder.BuildAsync("rome", Start, Start.AddDays(4), PRICE_LEVEL.PRICE_LEVEL_MODERATE, "alice", null);

            await _search.Received(4).SearchFirstMatchAsync(
                Arg.Any<IEnumerable<string>>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task No_two_days_eat_at_the_same_place()
        {
            var route = await _builder.BuildAsync(
                "rome", Start, Start.AddDays(4), PRICE_LEVEL.PRICE_LEVEL_MODERATE, "alice", null);

            var eaten = route.TravelDays
                .SelectMany(day => new string?[] { day.Breakfast!.GoogleId, day.Lunch!.GoogleId, day.Dinner!.GoogleId })
                .ToList();

            eaten.Should().HaveCount(15).And.OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task A_plan_that_does_not_validate_is_refused()
        {
            _validator.Validate(Arg.Any<StandardRoute>())
                .Returns(new RoutePlanValidationResult(false, 16, 12, Array.Empty<string>(), new[] { "day 2 lunch" }));

            await _builder.Invoking(builder => Build())
                .Should().ThrowAsync<RouteGenerationException>()
                .WithMessage("*day 2 lunch*");
        }

        [Fact]
        public async Task The_builder_does_not_save_anything_itself()
        {
            (await Build()).User.Should().BeNull();
        }
    }
}
