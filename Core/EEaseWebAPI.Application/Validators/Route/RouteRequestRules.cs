using FluentValidation;

namespace EEaseWebAPI.Application.Validators.Route
{
    internal static class RouteRequestRules
    {
        public const int MaximumDays = 5;
        public const int MaximumCompanions = 4;

        public static IRuleBuilderOptions<T, string?> Destination<T>(IRuleBuilder<T, string?> rule) =>
            rule.NotEmpty().WithMessage("A destination is required.")
                .MaximumLength(100).WithMessage("The destination is too long.");

        public static void DateRange<T>(AbstractValidator<T> validator, Func<T, DateOnly?> start, Func<T, DateOnly?> end)
        {
            validator.RuleFor(request => start(request))
                .NotNull().WithMessage("A start date is required.");

            validator.RuleFor(request => end(request))
                .NotNull().WithMessage("An end date is required.");

            validator.RuleFor(request => request)
                .Must(request => IsNotInThePast(start(request), end(request)))
                .WithMessage("A route cannot be created for the past.")
                .Must(request => IsWithinAYear(start(request), end(request)))
                .WithMessage("Routes cannot be created for dates more than 1 year away.")
                .Must(request => IsOrdered(start(request), end(request)))
                .WithMessage("The start date cannot be later than the end date.")
                .Must(request => IsWithinDayLimit(start(request), end(request)))
                .WithMessage($"The route can be a maximum of {MaximumDays} days.")
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
