using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using EEaseWebAPI.Application.Resources;
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
                .NotEmpty().WithMessage(ValidationMessages.Route_UsernameRequired);

            RuleFor(request => request.usernames)
                .Must(usernames => usernames == null || usernames.Count <= RouteRequestRules.MaximumCompanions)
                .WithMessage(string.Format(ValidationMessages.Route_TooManyCompanions, RouteRequestRules.MaximumCompanions));

            RuleForEach(request => request.usernames)
                .NotEmpty().WithMessage(ValidationMessages.Route_CompanionUsernameEmpty);
        }
    }
}
