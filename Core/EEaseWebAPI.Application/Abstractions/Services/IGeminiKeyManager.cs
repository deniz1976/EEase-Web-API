namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGeminiKeyManager
    {
        Task<string> GetAvailableApiKey();

        Task MarkKeyAsUsed(string apiKey);

        Task ReleaseKey(string apiKey);
    }
} 