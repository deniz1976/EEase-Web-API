using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.Place;
using EEaseWebAPI.Application.Features.Queries.Place.GetPlacePhoto;
using EEaseWebAPI.Application.MapEntities;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class GetPlacePhotoQueryHandlerTests
    {
        private const string PhotoName = "places/ChIJ-place/photos/AeJbb3e-photo";

        private readonly IGooglePlacesService _googlePlaces = Substitute.For<IGooglePlacesService>();
        private readonly IHeaderService _headers = Substitute.For<IHeaderService>();
        private readonly GetPlacePhotoQueryHandler _handler;

        public GetPlacePhotoQueryHandlerTests()
        {
            _headers.HeaderCreate(Arg.Any<int>()).Returns(new Header());
            _handler = new GetPlacePhotoQueryHandler(_headers, _googlePlaces);
        }

        private static GetPlacePhotoQueryRequest Request(
            string photoName = PhotoName, int width = 400, int height = 400) =>
            new() { PhotoName = photoName, MaxWidthPx = width, MaxHeightPx = height };

        [Fact]
        public async Task A_photo_is_asked_for_at_the_requested_size()
        {
            _googlePlaces
                .GetPlacePhotosAsync(PhotoName, 800, 600, Arg.Any<CancellationToken>())
                .Returns(new GetPlacePhotoQueryResponseBody { PhotoUri = "https://example.test/p.jpg" });

            var response = await _handler.Handle(Request(width: 800, height: 600), CancellationToken.None);

            response.Body!.PhotoUri.Should().Be("https://example.test/p.jpg");
        }

        [Fact]
        public async Task A_request_without_a_photo_name_is_refused()
        {
            // It used to leave with a bare Exception, which the caller read as a 500.
            await Assert.ThrowsAsync<ArgumentException>(
                () => _handler.Handle(Request(photoName: string.Empty), CancellationToken.None));
        }

        [Theory]
        [InlineData(0, 400)]
        [InlineData(400, 0)]
        [InlineData(4801, 400)]
        [InlineData(400, 4801)]
        public async Task A_size_outside_the_limits_is_refused(int width, int height)
        {
            await Assert.ThrowsAsync<PlacePhotoSizeOutOfRangeException>(
                () => _handler.Handle(Request(width: width, height: height), CancellationToken.None));
        }

        [Fact]
        public async Task Nothing_is_asked_of_google_when_the_request_is_refused()
        {
            await Assert.ThrowsAnyAsync<Exception>(
                () => _handler.Handle(Request(width: 0), CancellationToken.None));

            await _googlePlaces.DidNotReceiveWithAnyArgs().GetPlacePhotosAsync(default!);
        }
    }
}
