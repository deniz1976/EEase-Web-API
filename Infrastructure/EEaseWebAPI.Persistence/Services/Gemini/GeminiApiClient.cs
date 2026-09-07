using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.MapEntities.GeminiAI;
using EEaseWebAPI.Application.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace EEaseWebAPI.Persistence.Services.Gemini
{
    public sealed class GeminiApiClient : IGeminiApiClient
    {
        private static readonly JsonSerializerOptions ResponseSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private const string ModelOutputStepType = "model_output";
        private const string TextContentType = "text";

        private readonly HttpClient _httpClient;
        private readonly IGeminiKeyManager _keyManager;
        private readonly GeminiOptions _options;
        private readonly ILogger<GeminiApiClient> _logger;

        public GeminiApiClient(
            HttpClient httpClient,
            IGeminiKeyManager keyManager,
            IOptions<GeminiOptions> options,
            ILogger<GeminiApiClient> logger)
        {
            _httpClient = httpClient;
            _keyManager = keyManager;
            _options = options.Value;
            _logger = logger;
        }

        public async Task<string> GenerateContentAsync(
            string prompt,
            bool expectJson = false,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(prompt))
            {
                throw new GeminiInvalidMessageException("The prompt cannot be empty or whitespace.");
            }

            if (_options.ApiKeys.Count == 0)
            {
                throw new GeminiAPIKeyNotFoundException();
            }

            var requestBody = BuildRequestBody(prompt, expectJson);
            var attempt = 0;

            while (true)
            {
                attempt++;

                var apiKey = await _keyManager.AcquireKeyAsync(cancellationToken);

                using var request = CreateRequest(apiKey, requestBody);
                using var response = await _httpClient.SendAsync(request, cancellationToken);

                var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);

                if (response.IsSuccessStatusCode)
                {
                    return ExtractText(responseContent);
                }

                var quotaExceeded = response.StatusCode == HttpStatusCode.TooManyRequests;

                if (quotaExceeded)
                {
                    _keyManager.ReportQuotaExceeded(apiKey);
                }

                if (!ShouldRetry(response.StatusCode) || attempt > _options.MaxRetryCount)
                {
                    throw MapErrorResponse(response.StatusCode, responseContent);
                }

                var delay = quotaExceeded
                    ? TimeSpan.Zero
                    : TimeSpan.FromSeconds(Math.Pow(2, attempt - 1));

                _logger.LogWarning(
                    "Gemini request failed with {StatusCode}. Retrying in {Delay}s ({Attempt}/{Max}).",
                    (int)response.StatusCode,
                    delay.TotalSeconds,
                    attempt,
                    _options.MaxRetryCount);

                if (delay > TimeSpan.Zero)
                {
                    await Task.Delay(delay, cancellationToken);
                }
            }
        }

        private object BuildRequestBody(string prompt, bool expectJson)
        {
            var generationConfig = new Dictionary<string, object>
            {
                ["temperature"] = _options.Temperature,
                ["max_output_tokens"] = _options.MaxOutputTokens
            };

            var requestBody = new Dictionary<string, object>
            {
                ["model"] = _options.Model,
                ["input"] = prompt,
                ["generation_config"] = generationConfig
            };

            if (expectJson)
            {
                requestBody["response_format"] = new Dictionary<string, object>
                {
                    ["type"] = "text",
                    ["mime_type"] = "application/json"
                };
            }

            return requestBody;
        }

        private HttpRequestMessage CreateRequest(string apiKey, object requestBody)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, _options.InteractionsPath)
            {
                Content = JsonContent.Create(requestBody)
            };

            request.Headers.Add("x-goog-api-key", apiKey);
            request.Headers.Add("Api-Revision", _options.ApiRevision);
            request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return request;
        }

        private static bool ShouldRetry(HttpStatusCode statusCode) =>
            statusCode is HttpStatusCode.TooManyRequests
                or HttpStatusCode.InternalServerError
                or HttpStatusCode.BadGateway
                or HttpStatusCode.ServiceUnavailable
                or HttpStatusCode.GatewayTimeout;

        private static BaseException MapErrorResponse(HttpStatusCode statusCode, string responseContent)
        {
            var detail = Truncate(responseContent, 500);

            return statusCode switch
            {
                HttpStatusCode.TooManyRequests =>
                    new GeminiAPIKeyLimitExceededException($"Gemini quota exceeded. Response: {detail}"),

                HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden =>
                    new GeminiAPIKeyNotFoundException(),

                _ => new GeminiAIServiceException(
                    $"The Gemini API returned {(int)statusCode}. Response: {detail}")
            };
        }

        private static string ExtractText(string responseContent)
        {
            GeminiResponse? response;

            try
            {
                response = JsonSerializer.Deserialize<GeminiResponse>(responseContent, ResponseSerializerOptions);
            }
            catch (JsonException exception)
            {
                throw new GeminiAPIResponseParseException("Could not parse the Gemini response.", exception);
            }

            var text = string.Concat(
                (response?.Steps ?? Array.Empty<GeminiStep>())
                    .Where(step => string.Equals(step.Type, ModelOutputStepType, StringComparison.OrdinalIgnoreCase))
                    .SelectMany(step => step.Content ?? Array.Empty<GeminiStepContent>())
                    .Where(content => string.Equals(content.Type, TextContentType, StringComparison.OrdinalIgnoreCase))
                    .Select(content => content.Text));

            if (string.IsNullOrWhiteSpace(text))
            {
                throw new GeminiAPIResponseParseException(
                    $"No text was found in the Gemini response (status: {response?.Status ?? "unknown"}). " +
                    $"Response: {Truncate(responseContent, 500)}");
            }

            return text;
        }

        private static string Truncate(string value, int maxLength) =>
            value.Length <= maxLength ? value : value[..maxLength] + "…";
    }
}
