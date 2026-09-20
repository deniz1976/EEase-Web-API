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

                options.AddPolicy(RateLimitPolicies.Sensitive, context =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: ResolveClientKey(context),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            AutoReplenishment = true,
                            PermitLimit = limits.SensitivePermitLimit,
                            Window = TimeSpan.FromSeconds(limits.SensitiveWindowSeconds),
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
                        new ErrorResponse
                        {
                            Header = new Header
                            {
                                Success = false,
                                ResponseDate = DateTime.UtcNow,
                                EnumStatusCode = (int)StatusEnum.ServiceUnavailable
                            },
                            Body = new ErrorBody
                            {
                                StatusCode = StatusCodes.Status429TooManyRequests,
                                Title = "Too Many Requests",
                                Message = $"Rate limit exceeded. Try again in {retryAfterSeconds} seconds."
                            }
                        },
                        cancellationToken);
                };
            });

            return services;
        }

        /// <summary>
        /// Who the limit is counted against: the signed in user, or the address the request
        /// came from. Behind a reverse proxy every address would be the proxy's own, and the
        /// anonymous callers would share one bucket. The fix is UseForwardedHeaders with the
        /// proxy's address in KnownProxies - not without it, since a caller who is trusted to
        /// set X-Forwarded-For can give themselves a new bucket per request and pass through
        /// any limit at all. There is no proxy yet, so there is nothing to trust yet.
        /// </summary>
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
