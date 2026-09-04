using System.Net;
using System.Net.Mime;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.MapEntities;
using Microsoft.AspNetCore.Diagnostics;

namespace EEaseWebAPI.API.Extensions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var statusCode = ResolveStatusCode(exception);
            var enumStatusCode = exception is BaseException baseException
                ? baseException.EnumStatusCode ?? (int)StatusEnum.UnknownError
                : (int)StatusEnum.UnknownError;

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                _logger.LogError(
                    exception,
                    "Unhandled error. Path: {Path}, Method: {Method}",
                    httpContext.Request.Path,
                    httpContext.Request.Method);
            }
            else
            {
                _logger.LogWarning(
                    "Request rejected ({StatusCode}). Path: {Path}, Reason: {Reason}",
                    statusCode,
                    httpContext.Request.Path,
                    exception.Message);
            }

            httpContext.Response.StatusCode = statusCode;
            httpContext.Response.ContentType = MediaTypeNames.Application.Json;

            var payload = new GlobalError
            {
                StatusCode = statusCode,
                EnumStatusCode = enumStatusCode,
                Title = ReasonPhrase(statusCode),
                Message = ResolveMessage(exception, statusCode)
            };

            if (exception is RequestValidationException validationException)
            {
                await httpContext.Response.WriteAsJsonAsync(
                    new ValidationErrorResponse
                    {
                        StatusCode = payload.StatusCode,
                        EnumStatusCode = payload.EnumStatusCode,
                        Title = payload.Title,
                        Message = payload.Message,
                        Errors = validationException.Errors
                    },
                    cancellationToken);

                return true;
            }

            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        }

        private static int ResolveStatusCode(Exception exception) => exception switch
        {
            RequestValidationException => StatusCodes.Status400BadRequest,

            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

            Application.Exceptions.UserNotFoundException
                or Application.Exceptions.Login.UserNotFoundException
                or Application.Exceptions.UpdateUser.UserNotFoundException
                or RouteNotFoundException
                or FriendshipNotFoundException => StatusCodes.Status404NotFound,

            FriendRequestAlreadyExistsException
                or UserAlreadyBlockedException
                or Application.Exceptions.UpdateUser.UserAlreadyHasPreferencesException
                => StatusCodes.Status409Conflict,

            GeminiAPIKeyLimitExceededException => StatusCodes.Status429TooManyRequests,

            ExternalServiceNotConfiguredException => StatusCodes.Status503ServiceUnavailable,

            GeminiAIServiceException
                or GeminiAPIKeyNotFoundException
                or GeminiAPIResponseParseException => StatusCodes.Status502BadGateway,

            InvalidSearchTermException
                or NullArgumentException
                or ArgumentException
                or BaseException => StatusCodes.Status400BadRequest,

            OperationCanceledException => 499,

            _ => StatusCodes.Status500InternalServerError
        };

        private string ResolveMessage(Exception exception, int statusCode)
        {
            if (statusCode < StatusCodes.Status500InternalServerError)
            {
                return exception.Message;
            }

            return _environment.IsDevelopment()
                ? exception.ToString()
                : "An unexpected error occurred. Please try again later.";
        }

        private static string ReasonPhrase(int statusCode) =>
            Enum.IsDefined(typeof(HttpStatusCode), statusCode)
                ? ((HttpStatusCode)statusCode).ToString()
                : "Error";
    }

    public sealed class ValidationErrorResponse : GlobalError
    {
        public IReadOnlyDictionary<string, string[]> Errors { get; init; } =
            new Dictionary<string, string[]>();
    }
}
