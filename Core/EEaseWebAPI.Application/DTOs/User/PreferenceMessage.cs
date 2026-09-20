namespace EEaseWebAPI.Application.DTOs.User
{
    /// <summary>
    /// What the traveller said about themselves, in their own words. It travels in an object
    /// rather than as a bare JSON string so that a client does not have to remember the
    /// quotes, and so a second field can be added without breaking the first.
    /// </summary>
    public class PreferenceMessage
    {
        public string Message { get; set; } = string.Empty;
    }
}
