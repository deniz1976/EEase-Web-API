using FluentValidation;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Validators.Route
{
    internal static class RouteRequestRules
    {
        public const int MaximumDays = 5;
        public const int MaximumCompanions = 4;

        public static IRuleBuilderOptions<T, string?> Destination<T>(
            IRuleBuilder<T, string?> rule,
            IStringLocalizer<ValidationMessages> messages) =>
            rule.NotEmpty().WithMessage(messages["Route_DestinationRequired"])
                .MaximumLength(100).WithMessage(messages["Route_DestinationTooLong"]);

        public static void DateRange<T>(
            AbstractValidator<T> validator,
            Func<T, DateOnly?> start,
            Func<T, DateOnly?> end,
            IStringLocalizer<ValidationMessages> messages)
        {
            validator.RuleFor(request => start(request))
                .NotNull().WithMessage(messages["Route_StartDateRequired"]);

            validator.RuleFor(request => end(request))
                .NotNull().WithMessage(messages["Route_EndDateRequired"]);

            validator.RuleFor(request => request)
                .Must(request => IsNotInThePast(start(request), end(request)))
                .WithMessage(messages["Route_DateInThePast"])
                .Must(request => IsWithinAYear(start(request), end(request)))
                .WithMessage(messages["Route_DateTooFarAway"])
                .Must(request => IsOrdered(start(request), end(request)))
                .WithMessage(messages["Route_DatesOutOfOrder"])
                .Must(request => IsWithinDayLimit(start(request), end(request)))
                .WithMessage(messages["Route_TooManyDays", MaximumDays])
                .When(request => start(request).HasValue && end(request).HasValue);
        }

        private static bool IsNotInThePast(DateOnly? start, DateOnly? end)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);

            return start >= today && end >= today;
        }

        private static bool IsWithinAYear(DateOnly? start, DateOnly? end)
        {
            var limit = DateOnly.FromDateTime(DateTime.UtcNow).AddYears(1);

            return start <= limit && end <= limit;
        }

        private static bool IsOrdered(DateOnly? start, DateOnly? end) => start <= end;

        private static bool IsWithinDayLimit(DateOnly? start, DateOnly? end) =>
            end!.Value.DayNumber - start!.Value.DayNumber + 1 <= MaximumDays;
    }
}
