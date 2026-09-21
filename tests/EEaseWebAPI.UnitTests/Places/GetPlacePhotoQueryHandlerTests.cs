using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Features.Queries.Place.GetPlacePhoto;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.Validators.Place;
using FluentAssertions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Places
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

        [Fact]
        public async Task A_photo_is_asked_for_at_the_requested_size()
        {
            _googlePlaces
                .GetPlacePhotosAsync(PhotoName, 800, 600, Arg.Any<CancellationToken>())
                .Returns(new GetPlacePhotoQueryResponseBody { PhotoUri = "https://example.test/p.jpg" });

            var request = new GetPlacePhotoQueryRequest
            {
                PhotoName = PhotoName,
                MaxWidthPx = 800,
                MaxHeightPx = 600
            };

            var response = await _handler.Handle(request, CancellationToken.None);

            response.Body!.PhotoUri.Should().Be("https://example.test/p.jpg");
        }
    }

    public class GetPlacePhotoQueryValidatorTests
    {
        private const string PhotoName = "places/ChIJ-place/photos/AeJbb3e-photo";

        private readonly GetPlacePhotoQueryValidator _validator = new();

        private static GetPlacePhotoQueryRequest Request(
            string photoName = PhotoName, int width = 400, int height = 400) =>
            new() { PhotoName = photoName, MaxWidthPx = width, MaxHeightPx = height };

        [Fact]
        public void A_photo_name_of_the_right_shape_passes()
        {
            _validator.Validate(Request()).IsValid.Should().BeTrue();
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        [InlineData("not-a-photo-name")]
        [InlineData("places/only-a-place")]
        [InlineData("../../etc/passwd")]
        public void A_photo_name_of_any_other_shape_is_refused(string photoName)
        {
            var result = _validator.Validate(Request(photoName));

            result.IsValid.Should().BeFalse();
            result.Errors.Should().Contain(error => error.PropertyName == "PhotoName");
        }

        [Theory]
        [InlineData(0, 400)]
        [InlineData(400, 0)]
        [InlineData(-1, 400)]
        [InlineData(4801, 400)]
        [InlineData(400, 4801)]
        public void A_size_outside_the_limits_is_refused(int width, int height)
        {
            _validator.Validate(Request(width: width, height: height)).IsValid.Should().BeFalse();
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(4800, 4800)]
        public void A_size_on_the_edge_of_the_limits_is_allowed(int width, int height)
        {
            _validator.Validate(Request(width: width, height: height)).IsValid.Should().BeTrue();
        }
    }
}
