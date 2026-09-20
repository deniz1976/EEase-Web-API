using System.Net;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Persistence.Services.GooglePlaces;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class GooglePlacesServiceTests
    {
        private const string PhotoName = "places/ChIJ-place_id/photos/AeJbb3e-photo_id";

        private static GooglePlacesService Create(
            StubHandler handler, string apiKey = "test-key") =>
            new(
                new HttpClient(handler) { BaseAddress = new Uri("https://places.googleapis.com/") },
                Options.Create(new GooglePlacesOptions { ApiKey = apiKey }));

        [Fact]
        public async Task A_missing_api_key_is_reported_before_anything_is_sent()
        {
            var handler = new StubHandler("{}");

            await Assert.ThrowsAsync<ExternalServiceNotConfiguredException>(
                () => Create(handler, apiKey: "  ").SearchPlacesAsync("museums in Rome"));

            handler.Requests.Should().BeEmpty();
        }

        [Fact]
        public async Task A_place_id_is_escaped_into_the_url()
        {
            var handler = new StubHandler("{}");

            await Create(handler).GetPlaceDetailsAsync("a place/../id");

            handler.Requests.Single().AbsolutePath.Should().Be("/v1/places/a%20place%2F..%2Fid");
        }

        [Theory]
        [InlineData("places/one/photos")]
        [InlineData("../v1/places/one")]
        [InlineData("places/one/photos/two?key=stolen")]
        public async Task A_photo_name_that_is_not_one_of_googles_is_refused(string photoName)
        {
            // It is pasted into a URL that carries our API key, so only the shape Google
            // hands out is allowed through.
            var handler = new StubHandler("{}");

            await Assert.ThrowsAsync<ArgumentException>(
                () => Create(handler).GetPlacePhotosAsync(photoName));

            handler.Requests.Should().BeEmpty();
        }

        [Fact]
        public async Task A_real_photo_name_is_asked_for_at_the_requested_size()
        {
            var handler = new StubHandler("""{"photoUri":"https://example.test/photo.jpg"}""");

            var photo = await Create(handler).GetPlacePhotosAsync(PhotoName, maxWidth: 800, maxHeight: 600);

            photo.photoUri.Should().Be("https://example.test/photo.jpg");
            handler.Requests.Single().Query.Should().Contain("maxHeightPx=600").And.Contain("maxWidthPx=800");
        }

        [Fact]
        public async Task A_failed_call_says_what_google_answered()
        {
            var handler = new StubHandler("quota exhausted", HttpStatusCode.TooManyRequests);

            var act = () => Create(handler).SearchPlacesAsync("museums in Rome");

            (await act.Should().ThrowAsync<HttpRequestException>())
                .Which.Message.Should().Contain("429").And.Contain("quota exhausted");
        }

        [Fact]
        public async Task A_cancelled_caller_stops_the_call_to_google()
        {
            using var cancellation = new CancellationTokenSource();
            cancellation.Cancel();

            var handler = new StubHandler("{}");

            await Assert.ThrowsAnyAsync<OperationCanceledException>(
                () => Create(handler).SearchPlacesAsync("museums in Rome", cancellation.Token));
        }

        private sealed class StubHandler : HttpMessageHandler
        {
            private readonly string _body;
            private readonly HttpStatusCode _statusCode;

            public StubHandler(string body, HttpStatusCode statusCode = HttpStatusCode.OK)
            {
                _body = body;
                _statusCode = statusCode;
            }

            public List<Uri> Requests { get; } = new();

            protected override Task<HttpResponseMessage> SendAsync(
                HttpRequestMessage request, CancellationToken cancellationToken)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Requests.Add(request.RequestUri!);

                return Task.FromResult(new HttpResponseMessage(_statusCode)
                {
                    Content = new StringContent(_body)
                });
            }
        }
    }
}
