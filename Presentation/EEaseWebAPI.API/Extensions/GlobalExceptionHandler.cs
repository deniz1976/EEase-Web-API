using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Friendship;
using EEaseWebAPI.Application.Exceptions.Cities;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.MapEntities;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;
using System.Net;
using System.Net.Mime;

namespace EEaseWebAPI.API.Extensions
{
    public sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;
        private readonly IHostEnvironment _environment;
        private readonly IStringLocalizer<ErrorMessages> _messages;

        public GlobalExceptionHandler(
            ILogger<GlobalExceptionHandler> logger,
            IHostEnvironment environment,
            IStringLocalizer<ErrorMessages> messages)
        {
            _logger = logger;
            _environment = environment;
            _messages = messages;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            var statusCode = ResolveStatusCode(exception);
            var carriedCode = (exception as BaseException)?.EnumStatusCode;
            var enumStatusCode = carriedCode ?? (int)StatusEnum.UnknownError;

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

            var payload = new ErrorResponse
            {
                Header = new Header
                {
                    Success = false,
                    ResponseDate = DateTime.UtcNow,
                    EnumStatusCode = enumStatusCode
                },
                Body = new ErrorBody
                {
                    StatusCode = statusCode,
                    Title = ReasonPhrase(statusCode),
                    Message = ResolveMessage(exception, carriedCode, statusCode),
                    Errors = (exception as RequestValidationException)?.Errors
                }
            };

            await httpContext.Response.WriteAsJsonAsync(payload, cancellationToken);
            return true;
        }

        private static int ResolveStatusCode(Exception exception) => exception switch
        {
            RequestValidationException => StatusCodes.Status400BadRequest,

            UnauthorizedAccessException => StatusCodes.Status401Unauthorized,

            ForbiddenException
                or DeleteRouteException => StatusCodes.Status403Forbidden,

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

            MailDeliveryException
                or GeminiAIServiceException
                or GeminiAPIKeyNotFoundException
                or GeminiAPIResponseParseException => StatusCodes.Status502BadGateway,

            InvalidSearchTermException
                or NullArgumentException
                or ArgumentException
                or BaseException => StatusCodes.Status400BadRequest,

            OperationCanceledException => 499,

            _ => StatusCodes.Status500InternalServerError
        };

        /// <summary>
        /// The message the caller reads. Exceptions carry a status code rather than a
        /// sentence, so the code is looked up in the caller's language; an exception whose
        /// code has no translation keeps the message it was thrown with, which is often a
        /// detail no resource file could hold ("no hotel could be found in Rome").
        /// </summary>
        private string ResolveMessage(Exception exception, int? carriedCode, int statusCode)
        {
            // An exception that carries a code is one we chose to throw, so the caller is
            // told about it in their own language whatever the status code is. A mail that
            // could not be sent is a 502, but it is still a sentence we wrote for them.
            if (carriedCode is not null)
            {
                return Translate((StatusEnum)carriedCode.Value) ?? exception.Message;
            }

            if (statusCode >= StatusCodes.Status500InternalServerError)
            {
                return _environment.IsDevelopment()
                    ? exception.ToString()
                    : Translate(StatusEnum.UnknownError) ?? "An unexpected error occurred. Please try again later.";
            }

            // Reading a missing code as UnknownError would answer "an unexpected error
            // occurred" to a caller who was simply told their username was taken.
            return exception.Message;
        }

        private string? Translate(StatusEnum statusEnum)
        {
            if (!Enum.IsDefined(statusEnum))
            {
                return null;
            }

            var translation = _messages[statusEnum.ToString()];

            return translation.ResourceNotFound ? null : translation.Value;
        }

        private static string ReasonPhrase(int statusCode) =>
            Enum.IsDefined(typeof(HttpStatusCode), statusCode)
                ? ((HttpStatusCode)statusCode).ToString()
                : "Error";
    }
}
