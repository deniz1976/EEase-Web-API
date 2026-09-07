using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PreferenceProfileBuilderTests
    {
        private readonly PreferenceProfileBuilder _builder = new();

        [Fact]
        public void Nothing_selected_produces_an_empty_profile()
        {
            var profile = _builder.Build(
                new UserAccommodationPreferences(),
                new UserFoodPreferences(),
                new UserPersonalization());

            profile.IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void Missing_preference_entities_are_tolerated()
        {
            _builder.Build(null, null, null).IsEmpty.Should().BeTrue();
        }

        [Fact]
        public void A_vegan_user_gets_a_mandatory_vegan_preference()
        {
            var profile = _builder.Build(
                null,
                new UserFoodPreferences { VeganPreference = 10 },
                null);

            var vegan = profile.Food.Single(item => item.Name == "VeganPreference");

            vegan.IsMandatory.Should().BeTrue();
            vegan.Priority.Should().Be(100);
        }

        [Fact]
        public void Meat_based_food_is_dropped_for_a_vegan_user()
        {
            var profile = _builder.Build(
                null,
                new UserFoodPreferences { VeganPreference = 90, SeafoodPreference = 100 },
                null);

            profile.Food.Should().NotContain(item => item.Name == "SeafoodPreference");
        }

        [Fact]
        public void A_low_vegetarian_score_is_not_mandatory()
        {
            var profile = _builder.Build(
                null,
                new UserFoodPreferences { VegetarianPreference = 50, SeafoodPreference = 100 },
                null);

            profile.Food.Single(item => item.Name == "VegetarianPreference").IsMandatory.Should().BeFalse();
            profile.Food.Should().Contain(item => item.Name == "SeafoodPreference");
        }

        [Fact]
        public void A_dominant_preference_is_boosted_above_a_plain_one()
        {
            var profile = _builder.Build(
                new UserAccommodationPreferences { PetFriendlyPreference = 50, HostelPreference = 50 },
                null,
                null);

            var pet = profile.Accommodation.Single(item => item.Name == "PetFriendlyPreference");
            var hostel = profile.Accommodation.Single(item => item.Name == "HostelPreference");

            pet.IsDominant.Should().BeTrue();
            pet.Score.Should().Be(hostel.Score);
            pet.Weight.Should().Be(hostel.Weight * 2);
        }

        [Fact]
        public void Vegan_doubles_the_score_of_compatible_food()
        {
            var profile = _builder.Build(
                null,
                new UserFoodPreferences { VeganPreference = 90, OrganicPreference = 40 },
                null);

            profile.Food.Single(item => item.Name == "OrganicPreference").Score.Should().Be(80);
        }

        [Fact]
        public void Preferences_of_several_users_are_merged()
        {
            var profile = _builder.Build(new[]
            {
                ((UserAccommodationPreferences?)new UserAccommodationPreferences { HostelPreference = 30 },
                 (UserFoodPreferences?)null,
                 (UserPersonalization?)null),
                (new UserAccommodationPreferences { VillaPreference = 40 }, null, null)
            });

            profile.Accommodation.Select(item => item.Name)
                .Should().BeEquivalentTo("HostelPreference", "VillaPreference");
        }

        [Fact]
        public void A_single_traveller_keeps_the_score_that_was_stored()
        {
            var profile = _builder.Build(
                new UserAccommodationPreferences { HostelPreference = 37 }, null, null);

            profile.Accommodation.Single().Score.Should().Be(37);
        }

        [Fact]
        public void A_preference_only_one_of_four_travellers_holds_is_diluted_but_not_erased()
        {
            var profile = _builder.Build(new[]
            {
                ((UserAccommodationPreferences?)new UserAccommodationPreferences { VillaPreference = 100 },
                 (UserFoodPreferences?)null, (UserPersonalization?)null),
                (new UserAccommodationPreferences(), null, null),
                (new UserAccommodationPreferences(), null, null),
                (new UserAccommodationPreferences(), null, null)
            });

            profile.Accommodation.Single(item => item.Name == "VillaPreference").Score.Should().Be(55);
        }

        [Fact]
        public void A_preference_the_whole_group_shares_beats_one_persons_favourite()
        {
            var mild = new UserAccommodationPreferences { HostelPreference = 60 };

            var profile = _builder.Build(new[]
            {
                ((UserAccommodationPreferences?)new UserAccommodationPreferences
                    { HostelPreference = 60, VillaPreference = 100 },
                 (UserFoodPreferences?)null, (UserPersonalization?)null),
                (mild, null, null),
                (mild, null, null)
            });

            var hostel = profile.Accommodation.Single(item => item.Name == "HostelPreference");
            var villa = profile.Accommodation.Single(item => item.Name == "VillaPreference");

            hostel.Score.Should().Be(60);
            villa.Score.Should().Be(60);
        }

        [Fact]
        public void A_barely_held_dominant_preference_cannot_outweigh_a_maxed_normal_one()
        {
            var profile = _builder.Build(
                new UserAccommodationPreferences { PetFriendlyPreference = 5, LuxuryHotelPreference = 100 },
                null,
                null);

            var pet = profile.Accommodation.Single(item => item.Name == "PetFriendlyPreference");
            var luxury = profile.Accommodation.Single(item => item.Name == "LuxuryHotelPreference");

            pet.Weight.Should().BeLessThan(luxury.Weight);
        }

        [Fact]
        public void One_vegan_in_the_group_makes_the_whole_group_vegan()
        {
            var profile = _builder.Build(new[]
            {
                ((UserAccommodationPreferences?)null,
                 (UserFoodPreferences?)new UserFoodPreferences { VeganPreference = 80 },
                 (UserPersonalization?)null),
                (null, new UserFoodPreferences { SeafoodPreference = 100 }, null),
                (null, new UserFoodPreferences { SeafoodPreference = 100 }, null)
            });

            profile.Food.Should().NotContain(item => item.Name == "SeafoodPreference");
            profile.Food.Single(item => item.Name == "VeganPreference").IsMandatory.Should().BeTrue();
        }

        [Fact]
        public void A_repeated_preference_is_merged_into_one_entry()
        {
            var profile = _builder.Build(new[]
            {
                ((UserAccommodationPreferences?)new UserAccommodationPreferences { HostelPreference = 40 },
                 (UserFoodPreferences?)null, (UserPersonalization?)null),
                (new UserAccommodationPreferences { HostelPreference = 60 }, null, null)
            });

            profile.Accommodation.Should().ContainSingle();
            profile.Accommodation.Single().Score.Should().Be(54);
        }

        [Fact]
        public void Zero_scored_preferences_are_ignored()
        {
            var profile = _builder.Build(
                null,
                null,
                new UserPersonalization { BeachPreference = 0, CulturalPreference = 20 });

            profile.Personalization.Select(item => item.Name).Should().Equal("CulturalPreference");
        }
    }
}
