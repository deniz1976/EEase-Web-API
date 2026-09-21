using EEaseWebAPI.Persistence.Contexts;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EEaseWebAPI.IntegrationTests
{
    public class EEaseApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = $"integration-{Guid.NewGuid():N}";

        // The limiter cannot tell one test from another: a client the factory makes has no
        // remote address, so every anonymous call shares one bucket. Tests about anything
        // else raise the allowance out of the way; RateLimitTests puts it back.
        protected virtual int SensitivePermitLimit => 10_000;

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("Testing");

            builder.UseSetting("ConnectionStrings:PostgreSQL", "Host=unused;Database=unused;Username=unused;Password=unused");
            builder.UseSetting("Token:Issuer", "eease-tests");
            builder.UseSetting("Token:Audience", "eease-tests");
            builder.UseSetting("Token:SecurityKey", "integration-tests-signing-key-long-enough-for-hmac-sha256");
            builder.UseSetting("Database:MigrateOnStartup", "false");
            builder.UseSetting("RateLimiting:SensitivePermitLimit", SensitivePermitLimit.ToString());

            builder.ConfigureServices(services =>
            {
                var registration = services.Single(
                    service => service.ServiceType == typeof(DbContextOptions<EEaseAPIDbContext>));

                services.Remove(registration);

                services.AddDbContext<EEaseAPIDbContext>(
                    options => options.UseInMemoryDatabase(_databaseName));
            });
        }
    }

    public sealed class ThrottledApiFactory : EEaseApiFactory
    {
        protected override int SensitivePermitLimit => 10;
    }
}
