using EEaseWebAPI.Application.Features.Queries.Place.GetPlacePhoto;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.Place
{
    public class GetPlacePhotoQueryValidator : AbstractValidator<GetPlacePhotoQueryRequest>
    {
        public const int MaximumPixels = 4800;

        public GetPlacePhotoQueryValidator()
        {
            RuleFor(request => request.PhotoName)
                .NotEmpty().WithMessage(ValidationMessages.PhotoName_Required)
                .Matches(@"^places/[A-Za-z0-9_.\-]+/photos/[A-Za-z0-9_.\-]+$")
                .WithMessage(ValidationMessages.PhotoName_Invalid);

            RuleFor(request => request.MaxWidthPx)
                .InclusiveBetween(1, MaximumPixels).WithMessage(ValidationMessages.PhotoSize_OutOfRange);

            RuleFor(request => request.MaxHeightPx)
                .InclusiveBetween(1, MaximumPixels).WithMessage(ValidationMessages.PhotoSize_OutOfRange);
        }
    }
}
