using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class RoutePlanValidatorTests
    {
        private readonly RoutePlanValidator _validator = new();

        private static StandardRoute CreateRoute(int dayCount, Func<int, int, string> googleId)
        {
            var route = new StandardRoute { TravelDays = new List<TravelDay>() };

            for (var day = 0; day < dayCount; day++)
            {
                route.TravelDays.Add(new TravelDay
                {
                    Breakfast = new Breakfast { GoogleId = googleId(day, 0) },
                    Lunch = new Lunch { GoogleId = googleId(day, 1) },
                    Dinner = new Dinner { GoogleId = googleId(day, 2) },
                    FirstPlace = new Place { GoogleId = googleId(day, 3) },
                    SecondPlace = new Place { GoogleId = googleId(day, 4) },
                    ThirdPlace = new Place { GoogleId = googleId(day, 5) },
                    PlaceAfterDinner = new PlaceAfterDinner { GoogleId = googleId(day, 6) }
                });
            }

            return route;
        }

        [Fact]
        public void A_route_with_seven_distinct_places_per_day_is_valid()
        {
            var result = _validator.Validate(CreateRoute(3, (day, slot) => $"place-{day}-{slot}"));

            result.IsValid.Should().BeTrue();
            result.ExpectedPlaceCount.Should().Be(21);
            result.UniquePlaceCount.Should().Be(21);
        }

        [Fact]
        public void A_place_used_twice_makes_the_route_invalid()
        {
            var route = CreateRoute(2, (day, slot) => $"place-{day}-{slot}");
            route.TravelDays[1].Lunch!.GoogleId = route.TravelDays[0].Breakfast!.GoogleId;

            var result = _validator.Validate(route);

            result.IsValid.Should().BeFalse();
            result.DuplicateGoogleIds.Should().ContainSingle();
            result.Describe().Should().Contain("duplicate");
        }

        [Fact]
        public void An_unfilled_slot_makes_the_route_invalid()
        {
            var route = CreateRoute(1, (day, slot) => $"place-{day}-{slot}");
            route.TravelDays[0].PlaceAfterDinner = null;

            var result = _validator.Validate(route);

            result.IsValid.Should().BeFalse();
            result.EmptySlots.Should().ContainSingle().Which.Should().Contain("evening venue");
        }

        [Fact]
        public void A_blank_google_id_counts_as_unfilled()
        {
            var route = CreateRoute(1, (day, slot) => $"place-{day}-{slot}");
            route.TravelDays[0].Dinner!.GoogleId = "   ";

            var result = _validator.Validate(route);

            result.IsValid.Should().BeFalse();
            result.EmptySlots.Should().ContainSingle().Which.Should().Contain("dinner");
        }

        [Fact]
        public void A_route_without_days_is_invalid()
        {
            _validator.Validate(null).IsValid.Should().BeFalse();
            _validator.Validate(new StandardRoute { TravelDays = new List<TravelDay>() }).IsValid.Should().BeFalse();
        }
    }
}
