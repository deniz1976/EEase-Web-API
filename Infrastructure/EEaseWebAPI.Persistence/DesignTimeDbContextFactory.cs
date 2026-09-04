using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EEaseWebAPI.Persistence
{
    public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EEaseAPIDbContext>
    {
        public EEaseAPIDbContext CreateDbContext(string[] args)
        {
            var configuration = BuildConfiguration(args);
            var connectionString = configuration.GetConnectionString("PostgreSQL");

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No design-time connection string was found. Set the " +
                    "'ConnectionStrings__PostgreSQL' environment variable or fill in the " +
                    "API project's appsettings file.");
            }

            var builder = new DbContextOptionsBuilder<EEaseAPIDbContext>();
            builder.UseNpgsql(connectionString);

            return new EEaseAPIDbContext(builder.Options);
        }

        private static IConfigurationRoot BuildConfiguration(string[] args)
        {
            var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
            var apiProjectPath = ResolveApiProjectPath();

            var builder = new ConfigurationBuilder().SetBasePath(apiProjectPath);

            builder.AddJsonFile("appsettings.json", optional: true);
            builder.AddJsonFile($"appsettings.{environment}.json", optional: true);
            builder.AddEnvironmentVariables();

            if (args.Length > 0)
            {
                builder.AddCommandLine(args);
            }

            return builder.Build();
        }

        private static string ResolveApiProjectPath()
        {
            const string relativeApiPath = "Presentation/EEaseWebAPI.API";

            var directory = new DirectoryInfo(Directory.GetCurrentDirectory());

            while (directory is not null)
            {
                var candidate = Path.Combine(directory.FullName, relativeApiPath);

                if (Directory.Exists(candidate))
                {
                    return candidate;
                }

                directory = directory.Parent;
            }

            return Directory.GetCurrentDirectory();
        }
    }
}
