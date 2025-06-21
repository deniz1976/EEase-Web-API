using EEaseWebAPI.Application.Abstractions.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Tasks;

namespace EEaseWebAPI.Persistence.Extensions
{
    public static class CacheInitializationExtension
    {
        public static async Task InitializeCache(this IApplicationBuilder app)
        {
            using var scope = app.ApplicationServices.CreateScope();
            var userCacheService = scope.ServiceProvider.GetRequiredService<IUserCacheService>();
            await userCacheService.LoadUsersToCache();
        }
    }
} 