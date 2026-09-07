namespace EEaseWebAPI.Application.DTOs.Route
{
    public class PreferenceItem
    {
        public required string Name { get; init; }

        public int Score { get; set; }

        public double Weight { get; set; }

        public bool IsDominant { get; init; }

        public bool IsMandatory { get; set; }

        public int Priority { get; set; }
    }
}
