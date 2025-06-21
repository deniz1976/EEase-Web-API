using EEaseWebAPI.Application.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Services
{
    public class CacheInitializationService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public CacheInitializationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var userCacheService = scope.ServiceProvider.GetRequiredService<IUserCacheService>();
                await userCacheService.LoadUsersToCache();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Error initializing cache: {ex.Message}");
            }
        }
    }
} 