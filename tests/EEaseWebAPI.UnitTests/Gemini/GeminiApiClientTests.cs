using System.Net;
using System.Text.Json;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Persistence.Services.Gemini;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.Gemini
{
    public class GeminiApiClientTests
    {
        private const string InteractionResponse = """
        {
            "id": "v1_ChdtQnFiYXJIOEpjLUJ4czBQaHEtQmtBWRIX",
            "status": "completed",
            "usage": {
                "total_tokens": 178,
                "total_input_tokens": 3,
                "input_tokens_by_modality": [ { "modality": "text", "tokens": 3 } ],
                "total_cached_tokens": 0,
                "total_output_tokens": 9,
                "total_tool_use_tokens": 0,
                "total_thought_tokens": 166,
                "raw_prompt_token": 34,
                "model_invocation_token_counts": [
                    {
                        "prompt_tokens_details": [ { "modality": "text", "tokens": 34 } ],
                        "candidates_tokens_details": [ { "modality": "text", "tokens": 13 } ],
                        "thoughts_tokens_details": [ { "modality": "text", "tokens": 166 } ]
                    }
                ]
            },
            "created": "2026-09-04T19:23:04Z",
            "updated": "2026-09-04T19:23:04Z",
            "service_tier": "standard",
            "steps": [
                { "signature": "EroFCrcFARFNMg9d9i5QU4E5", "type": "thought" },
                {
                    "content": [ { "text": "Selam! Size nasıl yardımcı olabilirim?", "type": "text" } ],
                    "type": "model_output"
                }
            ],
            "object": "interaction",
            "model": "gemini-3.8-flash"
        }
        """;

        private static GeminiApiClient CreateClient(
            string responseBody,
            HttpStatusCode statusCode = HttpStatusCode.OK,
            StubHandler? handler = null)
        {
            var options = Options.Create(new GeminiOptions { ApiKeys = new[] { "test-key" } });

            var httpClient = new HttpClient(handler ?? new StubHandler(responseBody, statusCode))
            {
                BaseAddress = new Uri("https://generativelanguage.googleapis.com/")
            };

            return new GeminiApiClient(
                httpClient,
                new GeminiKeyManager(options),
                options,
                NullLogger<GeminiApiClient>.Instance);
        }

        [Fact]
        public async Task Posts_an_interaction_request_with_the_pinned_api_revision()
        {
            var handler = new StubHandler(InteractionResponse, HttpStatusCode.OK);
            var client = CreateClient(InteractionResponse, handler: handler);

            await client.GenerateContentAsync("merhaba", expectJson: true);

            handler.RequestUri!.AbsolutePath.Should().Be("/v1beta/interactions");
            handler.Headers!.GetValues("Api-Revision").Should().ContainSingle().Which.Should().Be("2026-05-20");
            handler.Headers.GetValues("x-goog-api-key").Should().ContainSingle().Which.Should().Be("test-key");

            using var body = JsonDocument.Parse(handler.RequestBody!);
            var root = body.RootElement;

            root.GetProperty("model").GetString().Should().Be("gemini-3.8-flash");
            root.GetProperty("input").GetString().Should().Be("merhaba");
            root.GetProperty("generation_config").GetProperty("temperature").GetDouble().Should().Be(1.0);
            root.GetProperty("generation_config").GetProperty("max_output_tokens").GetInt32().Should().Be(8192);
            root.GetProperty("response_format").GetProperty("mime_type").GetString().Should().Be("application/json");
        }

        [Fact]
        public async Task Omits_the_response_format_when_json_is_not_requested()
        {
            var handler = new StubHandler(InteractionResponse, HttpStatusCode.OK);
            var client = CreateClient(InteractionResponse, handler: handler);

            await client.GenerateContentAsync("merhaba");

            using var body = JsonDocument.Parse(handler.RequestBody!);

            body.RootElement.TryGetProperty("response_format", out _).Should().BeFalse();
        }

        [Fact]
        public async Task A_quota_breach_benches_the_key_and_retries_on_the_next_one()
        {
            var options = Options.Create(new GeminiOptions
            {
                ApiKeys = new[] { "first-key", "second-key" },
                RequestsPerMinutePerKey = 6000,
                QuotaCooldownSeconds = 600
            });

            var handler = new StubHandler(InteractionResponse, HttpStatusCode.OK)
            {
                FirstResponseStatusCode = HttpStatusCode.TooManyRequests
            };

            var client = new GeminiApiClient(
                new HttpClient(handler) { BaseAddress = new Uri("https://generativelanguage.googleapis.com/") },
                new GeminiKeyManager(options),
                options,
                NullLogger<GeminiApiClient>.Instance);

            var text = await client.GenerateContentAsync("merhaba");

            text.Should().Be("Selam! Size nasıl yardımcı olabilirim?");
            handler.UsedApiKeys.Should().Equal("first-key", "second-key");
        }

        [Fact]
        public async Task Reads_the_model_output_text_and_skips_thought_steps()
        {
            var client = CreateClient(InteractionResponse);

            var text = await client.GenerateContentAsync("merhaba");

            text.Should().Be("Selam! Size nasıl yardımcı olabilirim?");
        }

        [Fact]
        public async Task Throws_when_the_response_carries_no_model_output()
        {
            var client = CreateClient("""
            {
                "status": "failed",
                "object": "interaction",
                "steps": [ { "signature": "abc", "type": "thought" } ]
            }
            """);

            var exception = await Assert.ThrowsAsync<GeminiAPIResponseParseException>(
                () => client.GenerateContentAsync("merhaba"));

            exception.Message.Should().Contain("failed");
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly string _body;
            private readonly HttpStatusCode _statusCode;

            public StubHandler(string body, HttpStatusCode statusCode)
            {
                _body = body;
                _statusCode = statusCode;
            }

            public HttpStatusCode? FirstResponseStatusCode { get; init; }

            public List<string> UsedApiKeys { get; } = new();

            public Uri? RequestUri { get; private set; }

            public System.Net.Http.Headers.HttpRequestHeaders? Headers { get; private set; }

            public string? RequestBody { get; private set; }

            protected override async Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request,
                CancellationToken cancellationToken)
            {
                var statusCode = UsedApiKeys.Count == 0 && FirstResponseStatusCode is not null
                    ? FirstResponseStatusCode.Value
                    : _statusCode;

                UsedApiKeys.Add(request.Headers.GetValues("x-goog-api-key").Single());

                RequestUri = request.RequestUri;
                Headers = request.Headers;
                RequestBody = request.Content is null
                    ? null
                    : await request.Content.ReadAsStringAsync(cancellationToken);

                return new HttpResponseMessage(statusCode)
                {
                    Content = new StringContent(_body)
                };
            }
        }
    }
}
