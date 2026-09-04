using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class DatabaseOptions
    {
        public const string SectionName = "Database";

        [Range(5, 3600)]
        public int CommandTimeoutSeconds { get; init; } = 120;

        [Range(0, 10)]
        public int MaxRetryCount { get; init; } = 3;

        [Range(1, 120)]
        public int MaxRetryDelaySeconds { get; init; } = 30;

        public bool EnableSensitiveDataLogging { get; init; }

        public bool MigrateOnStartup { get; init; }
    }
}
