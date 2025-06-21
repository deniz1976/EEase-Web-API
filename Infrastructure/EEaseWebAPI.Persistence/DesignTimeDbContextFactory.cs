using EEaseWebAPI.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace EEaseWebAPI.Persistence
{
    public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<EEaseAPIDbContext>
    {
        public EEaseAPIDbContext CreateDbContext(string[] args)
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            var builder = new DbContextOptionsBuilder<EEaseAPIDbContext>();
            var connectionString = configuration.GetConnectionString("PostgreSQL");
            builder.UseNpgsql(connectionString);
            return new EEaseAPIDbContext(builder.Options);
        }
    }
}
