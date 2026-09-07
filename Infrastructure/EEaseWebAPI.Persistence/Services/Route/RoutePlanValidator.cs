using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class RoutePlanValidator : IRoutePlanValidator
    {
        private const int PlacesPerDay = 7;

        public RoutePlanValidationResult Validate(StandardRoute? route)
        {
            if (route?.TravelDays == null || route.TravelDays.Count == 0)
            {
                return RoutePlanValidationResult.Empty;
            }

            var expected = route.TravelDays.Count * PlacesPerDay;
            var occurrences = new Dictionary<string, List<string>>();
            var emptySlots = new List<string>();

            for (var index = 0; index < route.TravelDays.Count; index++)
            {
                var day = route.TravelDays[index];
                var dayNumber = index + 1;

                Record(occurrences, emptySlots, day?.Breakfast?.GoogleId, $"day {dayNumber} breakfast");
                Record(occurrences, emptySlots, day?.Lunch?.GoogleId, $"day {dayNumber} lunch");
                Record(occurrences, emptySlots, day?.Dinner?.GoogleId, $"day {dayNumber} dinner");
                Record(occurrences, emptySlots, day?.FirstPlace?.GoogleId, $"day {dayNumber} first place");
                Record(occurrences, emptySlots, day?.SecondPlace?.GoogleId, $"day {dayNumber} second place");
                Record(occurrences, emptySlots, day?.ThirdPlace?.GoogleId, $"day {dayNumber} third place");
                Record(occurrences, emptySlots, day?.PlaceAfterDinner?.GoogleId, $"day {dayNumber} evening venue");
            }

            var duplicates = occurrences
                .Where(entry => entry.Value.Count > 1)
                .Select(entry => $"{entry.Key} ({string.Join(", ", entry.Value)})")
                .ToArray();

            var isValid = emptySlots.Count == 0
                && duplicates.Length == 0
                && occurrences.Count == expected;

            return new RoutePlanValidationResult(
                isValid,
                expected,
                occurrences.Count,
                duplicates,
                emptySlots);
        }

        private static void Record(
            Dictionary<string, List<string>> occurrences,
            List<string> emptySlots,
            string? googleId,
            string slot)
        {
            if (string.IsNullOrWhiteSpace(googleId))
            {
                emptySlots.Add(slot);
                return;
            }

            if (!occurrences.TryGetValue(googleId, out var slots))
            {
                slots = new List<string>();
                occurrences[googleId] = slots;
            }

            slots.Add(slot);
        }
    }
}
