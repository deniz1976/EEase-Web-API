namespace EEaseWebAPI.Application.DTOs.Route
{
    public sealed record PreferenceFeedbackResult(
        string Category,
        IReadOnlyDictionary<string, int> Changes)
    {
        public static PreferenceFeedbackResult None { get; } =
            new("none", new Dictionary<string, int>());

        public bool HasChanges => Changes.Count > 0;

        public string Describe() =>
            HasChanges
                ? string.Join(", ", Changes.Select(change =>
                    $"{change.Key} ({(change.Value >= 0 ? "+" : string.Empty)}{change.Value})"))
                : "no preference changed";
    }
}
