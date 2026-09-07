using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Application.Validators.Route;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class RouteRequestValidatorTests
    {
        private static readonly DateOnly Today = DateOnly.FromDateTime(DateTime.UtcNow);

        private readonly CreateCustomRouteCommandValidator _customValidator = new();
        private readonly CreateRouteWithoutLoginCommandValidator _anonymousValidator = new();

        private static CreateCustomRouteCommandRequest CustomRequest(
            DateOnly? start = null,
            DateOnly? end = null,
            List<string>? usernames = null) =>
            new()
            {
                destination = "Lisbon",
                username = "alice",
                usernames = usernames,
                StartDate = start ?? Today.AddDays(1),
                EndDate = end ?? Today.AddDays(3)
            };

        [Fact]
        public void A_solo_route_without_companions_is_accepted()
        {
            _customValidator.Validate(CustomRequest(usernames: null)).IsValid.Should().BeTrue();
            _customValidator.Validate(CustomRequest(usernames: new List<string>())).IsValid.Should().BeTrue();
        }

        [Fact]
        public void More_than_four_companions_are_rejected()
        {
            var request = CustomRequest(usernames: new List<string> { "a", "b", "c", "d", "e" });

            _customValidator.Validate(request).IsValid.Should().BeFalse();
        }

        [Fact]
        public void Four_companions_are_accepted()
        {
            var request = CustomRequest(usernames: new List<string> { "a", "b", "c", "d" });

            _customValidator.Validate(request).IsValid.Should().BeTrue();
        }

        [Fact]
        public void A_route_in_the_past_is_rejected()
        {
            var result = _customValidator.Validate(CustomRequest(Today.AddDays(-1), Today.AddDays(1)));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.ErrorMessage.Contains("past"));
        }

        [Fact]
        public void An_end_date_before_the_start_date_is_rejected()
        {
            _customValidator.Validate(CustomRequest(Today.AddDays(5), Today.AddDays(2)))
                .IsValid.Should().BeFalse();
        }

        [Fact]
        public void Both_endpoints_allow_exactly_five_days_and_refuse_six()
        {
            var fiveDays = CustomRequest(Today.AddDays(1), Today.AddDays(5));
            var sixDays = CustomRequest(Today.AddDays(1), Today.AddDays(6));

            _customValidator.Validate(fiveDays).IsValid.Should().BeTrue();
            _customValidator.Validate(sixDays).IsValid.Should().BeFalse();

            _anonymousValidator.Validate(new CreateRouteWithoutLoginCommandRequest
            {
                destination = "Lisbon",
                StartDate = Today.AddDays(1),
                EndDate = Today.AddDays(5)
            }).IsValid.Should().BeTrue();

            _anonymousValidator.Validate(new CreateRouteWithoutLoginCommandRequest
            {
                destination = "Lisbon",
                StartDate = Today.AddDays(1),
                EndDate = Today.AddDays(6)
            }).IsValid.Should().BeFalse();
        }

        [Fact]
        public void A_missing_destination_is_rejected()
        {
            var request = CustomRequest();
            request.destination = "  ";

            _customValidator.Validate(request).IsValid.Should().BeFalse();
        }

        [Fact]
        public void A_date_more_than_a_year_away_is_rejected()
        {
            _anonymousValidator.Validate(new CreateRouteWithoutLoginCommandRequest
            {
                destination = "Lisbon",
                StartDate = Today.AddYears(1).AddDays(1),
                EndDate = Today.AddYears(1).AddDays(2)
            }).IsValid.Should().BeFalse();
        }
    }
}
