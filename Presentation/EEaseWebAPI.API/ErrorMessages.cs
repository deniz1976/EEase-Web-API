namespace EEaseWebAPI.API
{
    /// <summary>
    /// Marker for the error message resources in <c>Resources/ErrorMessages.*.resx</c>.
    /// The resource keys are <see cref="Application.Enums.StatusEnum"/> names, so an
    /// exception's status code is enough to look its message up in the caller's language.
    /// </summary>
    public sealed class ErrorMessages
    {
    }
}
