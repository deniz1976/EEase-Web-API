namespace EEaseWebAPI.Application.DTOs.Route
{
    public readonly record struct RouteAccessResult(bool IsAccessible, string? Message)
    {
        public static RouteAccessResult Allowed { get; } = new(true, null);

        public static RouteAccessResult Denied(string message) => new(false, message);
    }
}
