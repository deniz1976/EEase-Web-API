using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using FluentValidation;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Validators.Route
{
    public class CreateCustomRouteCommandValidator : AbstractValidator<CreateCustomRouteCommandRequest>
    {
        public CreateCustomRouteCommandValidator(IStringLocalizer<ValidationMessages> messages)
        {
            RouteRequestRules.Destination(RuleFor(request => request.destination), messages);
            RouteRequestRules.DateRange(this, request => request.StartDate, request => request.EndDate, messages);

            RuleFor(request => request.username)
                .NotEmpty().WithMessage(messages["Route_UsernameRequired"]);

            RuleFor(request => request.usernames)
                .Must(usernames => usernames == null || usernames.Count <= RouteRequestRules.MaximumCompanions)
                .WithMessage(messages["Route_TooManyCompanions", RouteRequestRules.MaximumCompanions]);

            RuleForEach(request => request.usernames)
                .NotEmpty().WithMessage(messages["Route_CompanionUsernameEmpty"]);
        }
    }
}
