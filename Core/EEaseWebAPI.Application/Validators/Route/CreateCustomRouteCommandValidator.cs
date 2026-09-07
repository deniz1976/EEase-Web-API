using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.Route
{
    public class CreateCustomRouteCommandValidator : AbstractValidator<CreateCustomRouteCommandRequest>
    {
        public CreateCustomRouteCommandValidator()
        {
            RouteRequestRules.Destination(RuleFor(request => request.destination));
            RouteRequestRules.DateRange(this, request => request.StartDate, request => request.EndDate);

            RuleFor(request => request.username)
                .NotEmpty().WithMessage("A username is required.");

            RuleFor(request => request.usernames)
                .Must(usernames => usernames == null || usernames.Count <= RouteRequestRules.MaximumCompanions)
                .WithMessage($"A route can be shared with at most {RouteRequestRules.MaximumCompanions} other people.");

            RuleForEach(request => request.usernames)
                .NotEmpty().WithMessage("A companion username cannot be empty.");
        }
    }
}
