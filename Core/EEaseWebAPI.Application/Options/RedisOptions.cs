using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class RedisOptions
    {
        public const string SectionName = "Redis";

        public string ConnectionString { get; init; } = string.Empty;

        [Required(AllowEmptyStrings = false)]
        public string InstanceName { get; init; } = "eease";

        [Range(1, 60)]
        public int ConnectTimeoutSeconds { get; init; } = 5;

        public bool IsEnabled => !string.IsNullOrWhiteSpace(ConnectionString);
    }
}
