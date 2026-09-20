using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Validators.Route
{
    public class CreateRouteWithoutLoginCommandValidator : AbstractValidator<CreateRouteWithoutLoginCommandRequest>
    {
        public CreateRouteWithoutLoginCommandValidator(IStringLocalizer<ValidationMessages> messages)
        {
            RouteRequestRules.Destination(RuleFor(request => request.destination), messages);
            RouteRequestRules.DateRange(this, request => request.StartDate, request => request.EndDate, messages);
        }
    }
}
