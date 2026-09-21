using EEaseWebAPI.Application.MapEntities.PreferenceGroups;
using EEaseWebAPI.Domain.Entities.Identity;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.User
{
    public class TravelPreferenceGroupsTests
    {
        private static readonly Type[] PreferenceRows =
        {
            typeof(UserAccommodationPreferences),
            typeof(UserFoodPreferences),
            typeof(UserPersonalization)
        };

        public static TheoryData<string, string> EveryPreferenceName()
        {
            var data = new TheoryData<string, string>();

            foreach (var (topic, names) in TravelPreferenceGroups.All)
            {
                foreach (var name in names)
                {
                    data.Add(topic, name);
                }
            }

            return data;
        }

        [Theory]
        [MemberData(nameof(EveryPreferenceName))]
        public void Every_name_in_the_table_is_owned_by_exactly_one_preference_row(string topic, string name)
        {
            var owners = PreferenceRows
                .Where(row => row.GetProperty(name)?.PropertyType == typeof(int?))
                .ToList();

            owners.Should().ContainSingle($"{topic} scores {name}");
        }

        [Fact]
        public void Every_topic_names_at_least_one_preference()
        {
            TravelPreferenceGroups.All
                .Where(group => group.Value.Count == 0)
                .Should().BeEmpty();
        }

        [Fact]
        public void A_topic_belongs_to_one_group_only()
        {
            var listed = TravelPreferenceGroups.AccommodationGroups.Groups.Keys
                .Concat(TravelPreferenceGroups.FoodGroups.Groups.Keys)
                .Concat(TravelPreferenceGroups.TravelGroups.Groups.Keys)
                .ToList();

            listed.Should().OnlyHaveUniqueItems();
            listed.Should().HaveCount(TravelPreferenceGroups.All.Count());
        }

        [Fact]
        public void A_topic_nobody_listed_is_not_found()
        {
            TravelPreferenceGroups.TryGetPreferenceNames("Not A Topic", out _).Should().BeFalse();
        }
    }
}
