using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Options;
using System.Globalization;
using System.Threading.RateLimiting;

namespace EEaseWebAPI.API.Extensions
{
    public static class RateLimitingExtensions
    {
        public static IServiceCollection AddApiRateLimiting(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            var limits = configuration.GetSection(RateLimitOptions.SectionName).Get<RateLimitOptions>()
                         ?? new RateLimitOptions();

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ResolveClientKey(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = limits.PermitLimit,
                            Window = TimeSpan.FromSeconds(limits.WindowSeconds),
                            QueueLimit = limits.QueueLimit
                        }));

                options.AddPolicy(RateLimitPolicies.Expensive, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ResolveClientKey(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = limits.ExpensivePermitLimit,
                            Window = TimeSpan.FromSeconds(limits.ExpensiveWindowSeconds),
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? (int)retryAfter.TotalSeconds
                        : limits.WindowSeconds;

                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.Headers.RetryAfter =
                        retryAfterSeconds.ToString(CultureInfo.InvariantCulture);

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new GlobalError
                        {
                            StatusCode = StatusCodes.Status429TooManyRequests,
                            EnumStatusCode = (int)StatusEnum.ServiceUnavailable,
                            Title = "Too Many Requests",
                            Message = $"Rate limit exceeded. Try again in {retryAfterSeconds} seconds."
                        },
                        cancellationToken);
                };
            });

            return services;
        }

        private static string ResolveClientKey(HttpContext context)
        {
            var userName = context.User?.Identity?.IsAuthenticated == true
                ? context.User.Identity.Name
                : null;

            if (!string.IsNullOrWhiteSpace(userName))
            {
                return $"user:{userName}";
            }

            var ipAddress = context.Connection.RemoteIpAddress?.ToString();
            return string.IsNullOrWhiteSpace(ipAddress) ? "anonymous" : $"ip:{ipAddress}";
        }
    }
}
