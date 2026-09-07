namespace EEaseWebAPI.Application.DTOs.Route
{
    public sealed record PreferenceProfile(
        IReadOnlyList<PreferenceItem> Accommodation,
        IReadOnlyList<PreferenceItem> Food,
        IReadOnlyList<PreferenceItem> Personalization)
    {
        public static PreferenceProfile Empty { get; } =
            new(Array.Empty<PreferenceItem>(), Array.Empty<PreferenceItem>(), Array.Empty<PreferenceItem>());

        public bool IsEmpty =>
            Accommodation.Count == 0 && Food.Count == 0 && Personalization.Count == 0;
    }
}
