using System.Diagnostics;
using EEaseWebAPI.Application.Security;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Application
{
    public class SecretCodeTests
    {
        [Theory]
        [InlineData("123456", "123456", true)]
        [InlineData("123456", "123457", false)]
        [InlineData("123456", "023456", false)]
        [InlineData("123456", "12345", false)]
        [InlineData("123456", "1234567", false)]
        [InlineData("123456", "", false)]
        [InlineData("", "", true)]
        public void A_code_matches_only_itself(string stored, string offered, bool matches)
        {
            SecretCode.Matches(stored, offered).Should().Be(matches);
        }

        [Fact]
        public void Nothing_matches_a_code_that_was_never_stored()
        {
            SecretCode.Matches(null, "123456").Should().BeFalse();
            SecretCode.Matches("123456", null).Should().BeFalse();
            SecretCode.Matches(null, null).Should().BeFalse();
        }

        [Fact]
        public void A_wrong_first_digit_takes_as_long_as_a_wrong_last_one()
        {
            const string stored = "123456";

            var firstDigitWrong = TimeOf(() => SecretCode.Matches(stored, "923456"));
            var lastDigitWrong = TimeOf(() => SecretCode.Matches(stored, "123459"));

            var difference = Math.Abs(firstDigitWrong - lastDigitWrong);
            var larger = Math.Max(firstDigitWrong, lastDigitWrong);

            // Loose on purpose: this is a scheduler on a build agent, not a laboratory. What
            // it would catch is the shape of the old comparison, which returned on the first
            // differing byte and so took a fraction of the time for a wrong first digit.
            difference.Should().BeLessThan(larger * 0.9);
        }

        private static double TimeOf(Func<bool> comparison)
        {
            for (var warmUp = 0; warmUp < 1_000; warmUp++)
            {
                comparison();
            }

            var stopwatch = Stopwatch.StartNew();

            for (var run = 0; run < 200_000; run++)
            {
                comparison();
            }

            return stopwatch.Elapsed.TotalMilliseconds;
        }
    }
}
