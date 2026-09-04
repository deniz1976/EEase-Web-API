using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence.Extensions
{
    public static class DatabaseMigrationExtension
    {
        public static async Task MigrateDatabaseAsync(this WebApplication app)
        {
            var databaseOptions = app.Services.GetRequiredService<IOptions<DatabaseOptions>>().Value;

            if (!databaseOptions.MigrateOnStartup)
            {
                return;
            }

            await using var scope = app.Services.CreateAsyncScope();

            var logger = scope.ServiceProvider
                .GetRequiredService<ILoggerFactory>()
                .CreateLogger("DatabaseMigration");

            var context = scope.ServiceProvider.GetRequiredService<EEaseAPIDbContext>();

            var pending = (await context.Database.GetPendingMigrationsAsync()).ToArray();

            if (pending.Length == 0)
            {
                logger.LogInformation("The database schema is up to date; no migrations to apply.");
                return;
            }

            logger.LogInformation(
                "Applying {Count} migration(s): {Migrations}",
                pending.Length,
                string.Join(", ", pending));

            await context.Database.MigrateAsync();

            logger.LogInformation("Migrations applied successfully.");
        }
    }
}
