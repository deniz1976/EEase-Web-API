namespace EEaseWebAPI.Application.MapEntities.ResetUserPreferences
{
    public class ResetUserPreferencesResponse
    {
        public Header? Header { get; set; }
        public ResetUserPreferencesBody? Body { get; set; }
    }

    public class ResetUserPreferencesBody
    {
        public string? Message { get; set; }
    }
} 