using EEaseWebAPI.API.Extensions;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EEaseWebAPI.UnitTests.Api
{
    public class JwtConfigurationTests
    {
        private static void Configure(string issuer, string audience, string securityKey) =>
            new ServiceCollection().AddJwtAuthentication(
                new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Token:Issuer"] = issuer,
                    ["Token:Audience"] = audience,
                    ["Token:SecurityKey"] = securityKey
                }).Build());

        private static string Key(int bytes) => new('k', bytes);

        [Fact]
        public void A_key_long_enough_to_sign_with_is_accepted()
        {
            var configuring = () => Configure("eease", "eease", Key(AuthenticationExtensions.MinimumSecurityKeyBytes));

            configuring.Should().NotThrow();
        }

        [Fact]
        public void A_key_one_byte_too_short_is_refused_at_startup()
        {
            var configuring = () => Configure("eease", "eease", Key(AuthenticationExtensions.MinimumSecurityKeyBytes - 1));

            configuring.Should().Throw<InvalidOperationException>()
                .WithMessage("*SecurityKey*")
                .WithMessage("*31 bytes*");
        }

        [Theory]
        [InlineData("", "eease")]
        [InlineData("eease", "")]
        public void A_missing_issuer_or_audience_is_refused_at_startup(string issuer, string audience)
        {
            var configuring = () => Configure(issuer, audience, Key(64));

            configuring.Should().Throw<InvalidOperationException>().WithMessage("*Token:Issuer*");
        }

        [Fact]
        public void A_key_counted_in_characters_rather_than_bytes_does_not_slip_through()
        {
            // Sixteen characters, thirty-two bytes: long enough, and only because of how it
            // is measured.
            var configuring = () => Configure("eease", "eease", new string('é', 16));

            configuring.Should().NotThrow();
        }
    }
}
