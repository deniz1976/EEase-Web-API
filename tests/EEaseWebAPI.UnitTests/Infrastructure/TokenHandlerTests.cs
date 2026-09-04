using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Infrastructure.Services.Token;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;
using TokenOptions = EEaseWebAPI.Application.Options.TokenOptions;

namespace EEaseWebAPI.UnitTests.Infrastructure
{
    public class TokenHandlerTests
    {
        private const string SecurityKey = "unit-test-signing-key-at-least-32-characters-long";
        private const string Issuer = "eease-test";
        private const string Audience = "eease-test-audience";

        private static TokenHandler CreateHandler() =>
            new(Options.Create(new TokenOptions
            {
                SecurityKey = SecurityKey,
                Issuer = Issuer,
                Audience = Audience
            }));

        private static AppUser CreateUser() => new()
        {
            Id = "user-id",
            UserName = "testuser"
        };

        [Fact]
        public void Issued_token_carries_the_configured_issuer_and_audience()
        {
            var token = CreateHandler().CreateAccessToken(3600, CreateUser());

            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

            parsed.Issuer.Should().Be(Issuer);
            parsed.Audiences.Should().Contain(Audience);
        }

        [Fact]
        public void Issued_token_carries_the_user_name_and_identifier()
        {
            var token = CreateHandler().CreateAccessToken(3600, CreateUser());

            var parsed = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);

            parsed.Claims.Should().Contain(claim =>
                claim.Type == ClaimTypes.Name && claim.Value == "testuser");

            parsed.Claims.Should().Contain(claim =>
                claim.Type == ClaimTypes.NameIdentifier && claim.Value == "user-id");
        }

        [Fact]
        public void Every_token_carries_a_unique_jti()
        {
            var handler = CreateHandler();

            var first = new JwtSecurityTokenHandler().ReadJwtToken(
                handler.CreateAccessToken(3600, CreateUser()).AccessToken);
            var second = new JwtSecurityTokenHandler().ReadJwtToken(
                handler.CreateAccessToken(3600, CreateUser()).AccessToken);

            var firstJti = first.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;
            var secondJti = second.Claims.Single(claim => claim.Type == JwtRegisteredClaimNames.Jti).Value;

            firstJti.Should().NotBe(secondJti);
        }

        [Fact]
        public void Lifetime_matches_the_requested_number_of_seconds()
        {
            var before = DateTime.UtcNow;

            var token = CreateHandler().CreateAccessToken(3600, CreateUser());

            token.Expiration.Should().BeCloseTo(before.AddSeconds(3600), TimeSpan.FromSeconds(10));
        }

        [Fact]
        public void No_token_is_issued_when_the_user_name_is_empty()
        {
            var user = new AppUser { Id = "user-id", UserName = null };

            var act = () => CreateHandler().CreateAccessToken(3600, user);

            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void No_token_is_issued_when_the_user_is_null()
        {
            var act = () => CreateHandler().CreateAccessToken(3600, null!);

            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void Refresh_tokens_are_unique()
        {
            var handler = CreateHandler();

            var tokens = Enumerable.Range(0, 50)
                .Select(_ => handler.CreateRefreshToken())
                .ToList();

            tokens.Should().OnlyHaveUniqueItems();
            tokens.Should().AllSatisfy(token => Convert.FromBase64String(token).Should().HaveCount(32));
        }
    }
}
