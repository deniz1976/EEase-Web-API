using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IRoutePlanValidator
    {
        RoutePlanValidationResult Validate(StandardRoute? route);
    }

    public sealed record RoutePlanValidationResult(
        bool IsValid,
        int ExpectedPlaceCount,
        int UniquePlaceCount,
        IReadOnlyList<string> DuplicateGoogleIds,
        IReadOnlyList<string> EmptySlots)
    {
        public static RoutePlanValidationResult Empty { get; } =
            new(false, 0, 0, Array.Empty<string>(), Array.Empty<string>());

        public string Describe()
        {
            var problems = new List<string>();

            if (EmptySlots.Count > 0)
                problems.Add($"missing places: {string.Join(", ", EmptySlots)}");

            if (DuplicateGoogleIds.Count > 0)
                problems.Add($"duplicate places: {string.Join(", ", DuplicateGoogleIds)}");

            if (problems.Count == 0 && !IsValid)
                problems.Add($"only {UniquePlaceCount} of {ExpectedPlaceCount} places could be filled");

            return string.Join("; ", problems);
        }
    }
}
