namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGeminiKeyManager
    {
        Task<string> AcquireKeyAsync(CancellationToken cancellationToken = default);

        void ReportQuotaExceeded(string apiKey);
    }
}
