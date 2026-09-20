using System.Globalization;
using System.Text.Json;
using EEaseWebAPI.API;
using EEaseWebAPI.API.Extensions;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.UnitTests.Localization;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;
using UserNotFoundException = EEaseWebAPI.Application.Exceptions.Login.UserNotFoundException;

namespace EEaseWebAPI.UnitTests.Api
{
    public class GlobalExceptionHandlerTests
    {
        private readonly GlobalExceptionHandler _handler;

        public GlobalExceptionHandlerTests()
        {
            _handler = new GlobalExceptionHandler(
                NullLogger<GlobalExceptionHandler>.Instance,
                new ProductionEnvironment(),
                Localizers.For<ErrorMessages>());
        }

        private async Task<(int StatusCode, JsonElement Body)> HandleAsync(Exception exception, string? culture = null)
        {
            var previous = CultureInfo.CurrentUICulture;

            if (culture is not null)
            {
                CultureInfo.CurrentUICulture = new CultureInfo(culture);
            }

            try
            {
                var context = new DefaultHttpContext();
                context.Response.Body = new MemoryStream();

                (await _handler.TryHandleAsync(context, exception, CancellationToken.None)).Should().BeTrue();

                context.Response.Body.Position = 0;

                using var document = await JsonDocument.ParseAsync(context.Response.Body);

                return (context.Response.StatusCode, document.RootElement.Clone());
            }
            finally
            {
                CultureInfo.CurrentUICulture = previous;
            }
        }

        [Fact]
        public async Task A_signed_in_user_who_may_not_see_a_route_is_forbidden_not_unauthorized()
        {
            var (statusCode, body) = await HandleAsync(
                new ForbiddenException("This route is private", StatusEnum.UnauthorizedToViewRoute));

            statusCode.Should().Be(StatusCodes.Status403Forbidden);
            body.GetProperty("enumStatusCode").GetInt32().Should().Be((int)StatusEnum.UnauthorizedToViewRoute);
        }

        [Fact]
        public async Task Deleting_someone_elses_route_is_forbidden()
        {
            var (statusCode, _) = await HandleAsync(new DeleteRouteException());

            statusCode.Should().Be(StatusCodes.Status403Forbidden);
        }

        [Fact]
        public async Task A_request_without_an_identity_stays_unauthorized()
        {
            var (statusCode, _) = await HandleAsync(new UnauthorizedAccessException("not signed in"));

            statusCode.Should().Be(StatusCodes.Status401Unauthorized);
        }

        [Fact]
        public async Task A_missing_user_is_a_not_found()
        {
            var (statusCode, _) = await HandleAsync(new UserNotFoundException("User not found", 7));

            statusCode.Should().Be(StatusCodes.Status404NotFound);
        }

        [Fact]
        public async Task An_unexpected_fault_is_a_server_error()
        {
            var (statusCode, body) = await HandleAsync(new InvalidCastException("boom"));

            statusCode.Should().Be(StatusCodes.Status500InternalServerError);
            body.GetProperty("message").GetString().Should().NotContain("boom");
        }

        [Fact]
        public async Task The_message_is_written_in_the_language_the_caller_asked_for()
        {
            var (_, english) = await HandleAsync(new RouteNotFoundException("Route not found", 93), "en");
            var (_, turkish) = await HandleAsync(new RouteNotFoundException("Route not found", 93), "tr");

            english.GetProperty("message").GetString().Should().Be("Route not found.");
            turkish.GetProperty("message").GetString().Should().Be("Rota bulunamadı.");
        }

        [Fact]
        public async Task A_language_nobody_translated_falls_back_to_english()
        {
            var (_, body) = await HandleAsync(new RouteNotFoundException("Route not found", 93), "de");

            body.GetProperty("message").GetString().Should().Be("Route not found.");
        }

        [Fact]
        public async Task An_exception_whose_code_has_no_translation_keeps_its_own_detail()
        {
            var (_, body) = await HandleAsync(
                new RouteGenerationException("No hotel could be found in Rome."), "tr");

            body.GetProperty("message").GetString().Should().Be("No hotel could be found in Rome.");
        }

        [Fact]
        public async Task An_exception_that_carries_no_code_keeps_its_own_message()
        {
            // Reading the missing code as UnknownError turned every plain rejection into
            // "an unexpected error occurred", which tells the caller nothing.
            var (statusCode, body) = await HandleAsync(new ArgumentException("Username must be unique."), "tr");

            statusCode.Should().Be(StatusCodes.Status400BadRequest);
            body.GetProperty("message").GetString().Should().Be("Username must be unique.");
        }

        [Fact]
        public async Task A_gateway_failure_we_threw_ourselves_is_still_explained()
        {
            // 5xx answers are generic on purpose, but a typed exception is a sentence we
            // wrote for the caller, not a fault we failed to foresee.
            var (statusCode, body) = await HandleAsync(
                new MailDeliveryException(
                    "The verification code could not be sent.",
                    (int)StatusEnum.VerificationCodeSendFailed),
                "tr");

            statusCode.Should().Be(StatusCodes.Status502BadGateway);
            body.GetProperty("enumStatusCode").GetInt32()
                .Should().Be((int)StatusEnum.VerificationCodeSendFailed);
            body.GetProperty("message").GetString()
                .Should().Be("Doğrulama kodu gönderilemedi. Lütfen birazdan tekrar deneyin.");
        }

        [Fact]
        public async Task A_validation_failure_lists_the_fields_that_failed()
        {
            var errors = new Dictionary<string, string[]> { ["Email"] = new[] { "Email is required." } };

            var (statusCode, body) = await HandleAsync(new RequestValidationException(errors), "tr");

            statusCode.Should().Be(StatusCodes.Status400BadRequest);
            body.GetProperty("message").GetString().Should().Be("Gönderilen istek doğrulama kurallarını sağlamıyor.");
            body.GetProperty("errors").GetProperty("Email")[0].GetString().Should().Be("Email is required.");
        }

        private sealed class ProductionEnvironment : IHostEnvironment
        {
            public string EnvironmentName { get; set; } = Environments.Production;
            public string ApplicationName { get; set; } = "EEaseWebAPI.API";
            public string ContentRootPath { get; set; } = AppContext.BaseDirectory;
            public IFileProvider ContentRootFileProvider { get; set; } =
                new PhysicalFileProvider(AppContext.BaseDirectory);
        }
    }
}
