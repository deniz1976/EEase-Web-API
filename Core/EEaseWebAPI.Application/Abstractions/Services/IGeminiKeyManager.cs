/// <summary>
/// Manages Gemini AI API keys, including key rotation, usage tracking, and availability.
/// Ensures efficient distribution and management of API keys for the Gemini AI service.
/// </summary>
namespace EEaseWebAPI.Application.Abstractions.Services
{
    /// <summary>
    /// Retrieves an available API key from the key pool.
    /// </summary>
    /// <returns>A valid and available Gemini AI API key</returns>
    /// <remarks>
    /// - Implements key rotation strategy
    /// - Checks key usage limits
    /// - Handles key availability status
    /// - Returns least recently used available key
    /// </remarks>
    public interface IGeminiKeyManager
    {
        /// <summary>
        /// Retrieves an available API key from the key pool.
        /// </summary>
        /// <returns>A valid and available Gemini AI API key</returns>
        /// <remarks>
        /// - Implements key rotation strategy
        /// - Checks key usage limits
        /// - Handles key availability status
        /// - Returns least recently used available key
        /// </remarks>
        Task<string> GetAvailableApiKey();

        /// <summary>
        /// Marks an API key as currently in use.
        /// </summary>
        /// <param name="apiKey">The API key to mark as used</param>
        /// <remarks>
        /// - Updates key usage statistics
        /// - Tracks request timestamps
        /// - Manages concurrent usage
        /// </remarks>
        Task MarkKeyAsUsed(string apiKey);

        /// <summary>
        /// Releases an API key back to the available pool.
        /// </summary>
        /// <param name="apiKey">The API key to release</param>
        /// <remarks>
        /// - Updates key availability status
        /// - Resets usage counters if needed
        /// - Makes key available for future requests
        /// </remarks>
        Task ReleaseKey(string apiKey);
    }
} 