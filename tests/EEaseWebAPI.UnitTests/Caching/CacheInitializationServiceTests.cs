using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Persistence.Services.Caching;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Caching
{
    public class CacheInitializationServiceTests
    {
        private readonly IUserCacheService _users = Substitute.For<IUserCacheService>();
        private readonly ICityService _cities = Substitute.For<ICityService>();
        private readonly ICurrencyService _currencies = Substitute.For<ICurrencyService>();

        private CacheInitializationService Service()
        {
            var services = new ServiceCollection();

            services.AddScoped(_ => _users);
            services.AddScoped(_ => _cities);
            services.AddScoped(_ => _currencies);

            return new CacheInitializationService(
                services.BuildServiceProvider(), NullLogger<CacheInitializationService>.Instance);
        }

        [Fact]
        public async Task Every_cache_is_warmed_up_when_the_host_starts()
        {
            await Service().StartAsync(CancellationToken.None);

            await _users.Received(1).LoadUsersToCache(Arg.Any<CancellationToken>());
            await _cities.Received(1).InitializeCacheAsync(Arg.Any<CancellationToken>());
            await _currencies.Received(1).InitializeCacheAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task A_cache_that_could_not_be_warmed_up_does_not_stop_the_others()
        {
            _cities.InitializeCacheAsync(Arg.Any<CancellationToken>())
                .ThrowsAsync(new InvalidOperationException("the database is not up yet"));

            var starting = () => Service().StartAsync(CancellationToken.None);

            await starting.Should().NotThrowAsync(
                "a cold cache is read from the database on the first request, not a reason to refuse to start");

            await _currencies.Received(1).InitializeCacheAsync(Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task A_host_that_never_finishes_starting_does_not_keep_the_api_down()
        {
            _users.LoadUsersToCache(Arg.Any<CancellationToken>())
                .ThrowsAsync(new TimeoutException("redis is not answering"));

            var starting = () => Service().StartAsync(CancellationToken.None);

            await starting.Should().NotThrowAsync();
        }
    }
}
