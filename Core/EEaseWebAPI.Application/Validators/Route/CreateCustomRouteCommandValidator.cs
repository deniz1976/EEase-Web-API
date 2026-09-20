using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.Route
{
    public class CreateCustomRouteCommandValidator : AbstractValidator<CreateCustomRouteCommandRequest>
    {
        public CreateCustomRouteCommandValidator()
        {
            RouteRequestRules.Destination(RuleFor(request => request.Destination));
            RouteRequestRules.DateRange(this, request => request.StartDate, request => request.EndDate);

            RuleFor(request => request.Username)
                .NotEmpty().WithMessage(ValidationMessages.Route_UsernameRequired);

            RuleFor(request => request.Usernames)
                .Must(usernames => usernames == null || usernames.Count <= RouteRequestRules.MaximumCompanions)
                .WithMessage(string.Format(ValidationMessages.Route_TooManyCompanions, RouteRequestRules.MaximumCompanions));

            RuleForEach(request => request.Usernames)
                .NotEmpty().WithMessage(ValidationMessages.Route_CompanionUsernameEmpty);
        }
    }
}
