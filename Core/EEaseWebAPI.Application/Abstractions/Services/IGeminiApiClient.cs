namespace EEaseWebAPI.Application.Abstractions.Services
{
    public interface IGeminiApiClient
    {
        Task<string> GenerateContentAsync(
            string prompt,
            bool expectJson = false,
            CancellationToken cancellationToken = default);
    }
}
