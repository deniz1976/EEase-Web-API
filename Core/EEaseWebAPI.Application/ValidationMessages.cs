namespace EEaseWebAPI.Application
{
    /// <summary>
    /// Marker for the validator messages in <c>Resources/ValidationMessages.*.resx</c>.
    /// Validators are built per request, after the request culture has been set, so the
    /// messages come out in the language the caller asked for.
    /// </summary>
    public sealed class ValidationMessages
    {
    }
}
