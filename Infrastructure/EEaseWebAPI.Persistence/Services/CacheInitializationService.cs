using EEaseWebAPI.Application.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Services
{
    public sealed class CacheInitializationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CacheInitializationService> _logger;

        public CacheInitializationService(
            IServiceProvider serviceProvider,
            ILogger<CacheInitializationService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            await using var scope = _serviceProvider.CreateAsyncScope();

            await WarmUpAsync(
                "user",
                () => scope.ServiceProvider.GetRequiredService<IUserCacheService>().LoadUsersToCache());

            await WarmUpAsync(
                "city",
                () => scope.ServiceProvider.GetRequiredService<ICityService>().InitializeCacheAsync());

            await WarmUpAsync(
                "currency",
                () => scope.ServiceProvider.GetRequiredService<ICurrencyService>().InitializeCacheAsync());
        }

        private async Task WarmUpAsync(string cacheName, Func<Task> load)
        {
            try
            {
                await load();
                _logger.LogInformation("Loaded the {Cache} cache.", cacheName);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Could not load the {Cache} cache; the data will be read from the database on first request.",
                    cacheName);
            }
        }
    }
}
