using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.Route
{
    public class CreateRouteWithoutLoginCommandValidator : AbstractValidator<CreateRouteWithoutLoginCommandRequest>
    {
        public CreateRouteWithoutLoginCommandValidator()
        {
            RouteRequestRules.Destination(RuleFor(request => request.destination));
            RouteRequestRules.DateRange(this, request => request.StartDate, request => request.EndDate);
        }
    }
}
