using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Application.Behaviors
{
    public sealed class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
        where TRequest : notnull
    {
        private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

        public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
        {
            _logger = logger;
        }

        public async Task<TResponse> Handle(
            TRequest request,
            RequestHandlerDelegate<TResponse> next,
            CancellationToken cancellationToken)
        {
            var requestName = typeof(TRequest).Name;
            var timestamp = Stopwatch.GetTimestamp();

            try
            {
                var response = await next();

                _logger.LogInformation(
                    "{Request} handled in {Elapsed:0} ms.",
                    requestName,
                    Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds);

                return response;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    "{Request} failed after {Elapsed:0} ms with {Exception}.",
                    requestName,
                    Stopwatch.GetElapsedTime(timestamp).TotalMilliseconds,
                    exception.GetType().Name);

                throw;
            }
        }
    }
}
