using EEaseWebAPI.Application.Behaviors;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using FluentAssertions;
using FluentValidation;
using MediatR;
using Xunit;

namespace EEaseWebAPI.UnitTests.Application
{
    public class ValidationBehaviorTests
    {
        private sealed record SampleRequest(string? Name, int Age) : IRequest<string>;

        private sealed class SampleValidator : AbstractValidator<SampleRequest>
        {
            public SampleValidator()
            {
                RuleFor(request => request.Name).NotEmpty().WithMessage("Name is required.");
                RuleFor(request => request.Age).GreaterThan(0).WithMessage("Age must be greater than zero.");
            }
        }

        private static Task<string> Next() => Task.FromResult("handled");

        [Fact]
        public async Task Handler_runs_when_there_is_no_validator()
        {
            var behavior = new ValidationBehavior<SampleRequest, string>([]);

            var result = await behavior.Handle(new SampleRequest("Ada", 30), Next, CancellationToken.None);

            result.Should().Be("handled");
        }

        [Fact]
        public async Task Valid_request_reaches_the_handler()
        {
            var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator()]);

            var result = await behavior.Handle(new SampleRequest("Ada", 30), Next, CancellationToken.None);

            result.Should().Be("handled");
        }

        [Fact]
        public async Task Handler_never_runs_for_an_invalid_request()
        {
            var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator()]);
            var handlerCalled = false;

            Task<string> next()
            {
                handlerCalled = true;
                return Task.FromResult("handled");
            }

            await Assert.ThrowsAsync<RequestValidationException>(() =>
                behavior.Handle(new SampleRequest(null, 0), next, CancellationToken.None));

            handlerCalled.Should().BeFalse();
        }

        [Fact]
        public async Task Errors_are_grouped_by_field_name()
        {
            var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator()]);

            var exception = await Assert.ThrowsAsync<RequestValidationException>(() =>
                behavior.Handle(new SampleRequest(null, -5), Next, CancellationToken.None));

            exception.Errors.Should().ContainKey("Name");
            exception.Errors.Should().ContainKey("Age");
            exception.Errors["Name"].Should().Contain("Name is required.");
            exception.EnumStatusCode.Should().Be((int)StatusEnum.ValidationError);
        }

        [Fact]
        public async Task Multiple_validators_run_together()
        {
            var extraValidator = new InlineValidator<SampleRequest>();
            extraValidator.RuleFor(request => request.Age).LessThan(120).WithMessage("Age must be less than 120.");

            var behavior = new ValidationBehavior<SampleRequest, string>([new SampleValidator(), extraValidator]);

            var exception = await Assert.ThrowsAsync<RequestValidationException>(() =>
                behavior.Handle(new SampleRequest("Ada", 500), Next, CancellationToken.None));

            exception.Errors["Age"].Should().Contain("Age must be less than 120.");
        }
    }
}
