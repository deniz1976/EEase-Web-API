using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Application.Options
{
    public sealed class RateLimitOptions
    {
        public const string SectionName = "RateLimiting";

        [Range(1, 100000)]
        public int PermitLimit { get; init; } = 100;

        [Range(1, 3600)]
        public int WindowSeconds { get; init; } = 30;

        [Range(0, 1000)]
        public int QueueLimit { get; init; }

        [Range(1, 10000)]
        public int ExpensivePermitLimit { get; init; } = 5;

        [Range(1, 3600)]
        public int ExpensiveWindowSeconds { get; init; } = 60;
    }
}
