using EEaseWebAPI.Application.Common;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class PreferenceScoringTests
    {
        [Fact]
        public void A_first_like_moves_an_unknown_preference_a_quarter_of_the_way()
        {
            PreferenceScoring.Reinforce(null).Should().Be(25);
            PreferenceScoring.Reinforce(0).Should().Be(25);
        }

        [Fact]
        public void Liking_slows_down_as_the_score_approaches_the_top()
        {
            var first = PreferenceScoring.Reinforce(0) - 0;
            var later = PreferenceScoring.Reinforce(90) - 90;

            later.Should().BeLessThan(first);
        }

        [Fact]
        public void Liking_never_pushes_past_the_maximum()
        {
            PreferenceScoring.Reinforce(100).Should().Be(100);
            PreferenceScoring.Reinforce(99).Should().Be(100);
        }

        [Fact]
        public void Disliking_pulls_the_score_back_down()
        {
            PreferenceScoring.Weaken(100).Should().Be(75);
            PreferenceScoring.Weaken(40).Should().Be(30);
        }

        [Fact]
        public void Disliking_never_goes_below_zero()
        {
            PreferenceScoring.Weaken(0).Should().Be(0);
            PreferenceScoring.Weaken(1).Should().Be(0);
        }

        [Fact]
        public void A_like_always_moves_at_least_one_point()
        {
            PreferenceScoring.Reinforce(98).Should().BeGreaterThan(98);
        }

        [Fact]
        public void Repeated_likes_converge_instead_of_saturating_immediately()
        {
            var score = 0;

            for (var i = 0; i < 5; i++)
            {
                score = PreferenceScoring.Reinforce(score);
            }

            score.Should().BeInRange(70, 85);
        }

        [Fact]
        public void A_dislike_undoes_roughly_one_like()
        {
            var liked = PreferenceScoring.Reinforce(50);

            PreferenceScoring.Weaken(liked).Should().BeLessThan(50);
        }

        [Fact]
        public void A_score_outside_the_range_is_pulled_back_in()
        {
            PreferenceScoring.Reinforce(500).Should().Be(100);
            PreferenceScoring.Weaken(-20).Should().Be(0);
        }
    }
}
