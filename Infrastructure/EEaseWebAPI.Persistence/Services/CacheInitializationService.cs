using EEaseWebAPI.Application.Abstractions.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Services
{
    public class CacheInitializationService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;

        public CacheInitializationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            using (var scope = _serviceProvider.CreateScope())
            {
                var cityService = scope.ServiceProvider.GetRequiredService<ICityService>();
                var currencyService = scope.ServiceProvider.GetRequiredService<ICurrencyService>();

                await cityService.InitializeCacheAsync();
                await currencyService.InitializeCacheAsync();
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }
} 