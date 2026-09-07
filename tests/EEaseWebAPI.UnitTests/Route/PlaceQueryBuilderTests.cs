using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Domain.Enums;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PlaceQueryBuilderTests
    {
        private sealed class FixedRandom : Random
        {
            private readonly int _value;

            public FixedRandom(int value) => _value = value;

            public override int Next(int maxValue) => _value % Math.Max(maxValue, 1);

            public override int Next(int minValue, int maxValue) => Math.Min(_value, maxValue);
        }

        private static PlaceQueryBuilder Builder(int random = 0) => new(new FixedRandom(random));

        private static PreferenceItem Item(
            string name, int score, bool dominant = false, bool mandatory = false, int priority = 0) =>
            new()
            {
                Name = name,
                Score = score,
                Weight = score,
                IsDominant = dominant,
                IsMandatory = mandatory,
                Priority = priority
            };

        [Theory]
        [InlineData(PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE, "3")]
        [InlineData(PRICE_LEVEL.PRICE_LEVEL_MODERATE, "4")]
        [InlineData(PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE, "5")]
        public void Hotel_stars_follow_the_price_level(PRICE_LEVEL level, string expected)
        {
            Builder().HotelStars(level).Should().Be(expected);
        }

        [Fact]
        public void An_unknown_price_level_falls_back_to_a_moderate_query()
        {
            Builder().PricePrefix(null).Should().Be("Moderate ");
            Builder().HotelStars(null).Should().Be("4");
        }

        [Fact]
        public void Accommodation_without_preferences_is_just_a_star_rating()
        {
            Builder().Accommodation(null, PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE).Should().Be("5 star hotel");
        }

        [Fact]
        public void Accommodation_appends_the_selected_keyword()
        {
            var query = Builder().Accommodation(
                new[] { Item("HostelPreference", 100) },
                PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE);

            query.Should().Be("3 star hotel hostel");
        }

        [Fact]
        public void A_meal_query_carries_the_price_and_the_meal()
        {
            var query = Builder().Food(null, PRICE_LEVEL.PRICE_LEVEL_MODERATE, MealType.Breakfast);

            query.Should().Be("Moderate Breakfast restaurant");
        }

        [Fact]
        public void A_mandatory_dietary_restriction_wins_over_a_higher_scored_preference()
        {
            var query = Builder(99).Food(
                new[]
                {
                    Item("SpicyPreference", 100),
                    Item("VeganPreference", 10, dominant: true, mandatory: true, priority: 100)
                },
                PRICE_LEVEL.PRICE_LEVEL_MODERATE,
                MealType.Dinner);

            query.Should().Be("Moderate Dinner restaurant vegan");
        }

        [Fact]
        public void The_strongest_restriction_is_used_when_several_apply()
        {
            var selected = Builder().SelectPreference(new[]
            {
                Item("GlutenFreePreference", 90, mandatory: true, priority: 80),
                Item("VeganPreference", 10, mandatory: true, priority: 100)
            });

            selected.Should().Be("VeganPreference");
        }

        [Fact]
        public void A_single_preference_is_not_enough_for_a_touristic_query()
        {
            Builder().Touristic(new[] { Item("BeachPreference", 100) })
                .Should().Be("Tourist attractions");
        }

        [Fact]
        public void A_touristic_query_uses_the_selected_preference()
        {
            var query = Builder().Touristic(new[]
            {
                Item("BeachPreference", 100),
                Item("CavePreference", 1)
            });

            query.Should().Be("Best beaches");
        }

        [Fact]
        public void An_unmapped_preference_falls_back_to_a_generic_touristic_query()
        {
            var query = Builder().Touristic(new[]
            {
                Item("MadeUpPreference", 100),
                Item("OtherPreference", 1)
            });

            query.Should().Be("Famous tourist attractions");
        }

        [Fact]
        public void An_after_dinner_query_without_preferences_still_names_a_venue()
        {
            var query = Builder().AfterDinner(null, PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE);

            query.Should().Be("Expensive Live music bars");
        }

        [Fact]
        public void An_after_dinner_query_uses_the_selected_preference()
        {
            var query = Builder().AfterDinner(
                new[] { Item("BeachPreference", 100) },
                PRICE_LEVEL.PRICE_LEVEL_MODERATE);

            query.Should().Be("Moderate Beach bars");
        }

        [Fact]
        public void An_alternative_touristic_query_avoids_the_preference_already_used()
        {
            var query = Builder().AlternativeTouristic(
                new[] { Item("BeachPreference", 100), Item("CavePreference", 1) },
                "Best beaches");

            query.Should().Be("Cave attractions");
        }

        [Fact]
        public void An_alternative_touristic_query_falls_back_when_every_preference_is_spent()
        {
            var query = Builder().AlternativeTouristic(
                new[] { Item("BeachPreference", 100), Item("CavePreference", 1) },
                "Beach and cave tour");

            query.Should().Be("Must visit spots");
        }

        [Fact]
        public void An_alternative_touristic_query_falls_back_with_too_few_preferences()
        {
            Builder().AlternativeTouristic(new[] { Item("BeachPreference", 100) }, "anything")
                .Should().Be("Must visit spots");
        }

        [Fact]
        public void An_alternative_after_dinner_query_carries_the_price()
        {
            Builder().AlternativeAfterDinner(PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE)
                .Should().Be("Inexpensive Quiet cafes");
        }

        [Theory]
        [InlineData(MealType.Breakfast, "Moderate Cafe")]
        [InlineData(MealType.Lunch, "Moderate Bistro")]
        [InlineData(MealType.Dinner, "Moderate Fine dining")]
        public void An_alternative_meal_query_matches_the_meal(MealType mealType, string expected)
        {
            Builder().AlternativeFood(mealType, PRICE_LEVEL.PRICE_LEVEL_MODERATE).Should().Be(expected);
        }

        [Fact]
        public void An_alternative_query_for_an_unmapped_meal_is_still_a_restaurant()
        {
            Builder().AlternativeFood(MealType.AfterDinner, PRICE_LEVEL.PRICE_LEVEL_MODERATE)
                .Should().Be("Moderate Restaurant");
        }

        [Fact]
        public void An_empty_preference_list_selects_nothing()
        {
            Builder().SelectPreference(Array.Empty<PreferenceItem>()).Should().BeEmpty();
        }

        [Fact]
        public void Zero_weighted_preferences_still_select_a_name()
        {
            Builder().SelectPreference(new[] { Item("HostelPreference", 0) })
                .Should().Be("HostelPreference");
        }
    }
}
