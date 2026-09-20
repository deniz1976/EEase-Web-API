using EEaseWebAPI.Application.Options;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.Application
{
    /// <summary>
    /// A setting that is out of range should stop the application from starting, not wait
    /// for the first request that reads it and answer that one with a 500.
    /// </summary>
    public class OptionsValidationTests
    {
        private static IStartupValidator StartupValidatorFor(params (string Key, string Value)[] settings)
        {
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(settings.ToDictionary(
                    setting => setting.Key, setting => (string?)setting.Value))
                .Build();

            var services = new ServiceCollection();
            services.AddApplicationOptions(configuration);

            return services.BuildServiceProvider().GetRequiredService<IStartupValidator>();
        }

        [Fact]
        public void The_defaults_are_enough_to_start()
        {
            var validate = () => StartupValidatorFor(
                ("Token:Issuer", "eease"),
                ("Token:Audience", "eease"),
                ("Token:SecurityKey", "a-signing-key-that-is-long-enough-for-hmac")).Validate();

            validate.Should().NotThrow();
        }

        [Fact]
        public void A_missing_signing_key_is_refused_before_the_first_request()
        {
            var validate = () => StartupValidatorFor(
                ("Token:Issuer", "eease"),
                ("Token:Audience", "eease")).Validate();

            validate.Should().Throw<OptionsValidationException>()
                .WithMessage("*SecurityKey*");
        }

        [Theory]
        [InlineData("MailService:Port", "99999")]
        [InlineData("MailService:TimeoutSeconds", "0")]
        [InlineData("CacheConfiguration:UserLifetimeHours", "0")]
        [InlineData("CacheConfiguration:AllCitiesCacheKey", "")]
        [InlineData("GooglePlaces:TimeoutSeconds", "1")]
        [InlineData("RateLimiting:PermitLimit", "0")]
        [InlineData("Database:CommandTimeoutSeconds", "1")]
        [InlineData("Redis:ConnectTimeoutSeconds", "600")]
        [InlineData("GeminiAI:Model", "")]
        public void A_section_nobody_used_to_check_is_checked_now(string key, string value)
        {
            var validate = () => StartupValidatorFor(
                ("Token:Issuer", "eease"),
                ("Token:Audience", "eease"),
                ("Token:SecurityKey", "a-signing-key-that-is-long-enough-for-hmac"),
                (key, value)).Validate();

            validate.Should().Throw<OptionsValidationException>();
        }
    }
}
