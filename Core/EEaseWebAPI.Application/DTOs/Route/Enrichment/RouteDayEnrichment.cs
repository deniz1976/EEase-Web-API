namespace EEaseWebAPI.Application.DTOs.Route.Enrichment
{
    public sealed class RouteDayEnrichment
    {
        public string? ApproxPrice { get; set; }

        public string? DayDescription { get; set; }

        public IReadOnlyList<WeatherForecastDto?> Weather { get; set; } = Array.Empty<WeatherForecastDto?>();

        public WeatherForecastDto? WeatherAt(RouteDaySlot slot)
        {
            var index = (int)slot;

            return index >= 0 && index < Weather.Count ? Weather[index] : null;
        }
    }

    public enum RouteDaySlot
    {
        Breakfast = 0,
        FirstPlace = 1,
        Lunch = 2,
        SecondPlace = 3,
        ThirdPlace = 4,
        Dinner = 5,
        AfterDinner = 6
    }
}
