using System.Diagnostics;

namespace EEaseWebAPI.API.Extensions
{
    public sealed class RequestLogScopeMiddleware : IMiddleware
    {
        private readonly ILogger<RequestLogScopeMiddleware> _logger;

        public RequestLogScopeMiddleware(ILogger<RequestLogScopeMiddleware> logger)
        {
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            // The framework already scopes the trace id, the connection and the path, so only
            // the caller is added here. The name is read when a log line is written rather
            // than now, because this middleware runs before authentication.
            using (_logger.BeginScope("UserName:{UserName}", new CurrentUserName(context)))
            {
                var timestamp = Stopwatch.GetTimestamp();

                try
                {
                    await next(context);
                }
                finally
                {
                    _logger.LogInformation(
                        "{Method} {Path} responded {StatusCode} in {Elapsed:0} ms.",
                        context.Request.Method,
                        context.Request.Path,
                        context.Response.StatusCode,
                        Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds);
                }
            }
        }

        private sealed class CurrentUserName
        {
            private readonly HttpContext _context;

            public CurrentUserName(HttpContext context) => _context = context;

            public override string ToString() => _context.User.Identity?.Name ?? "anonymous";
        }
    }
}
