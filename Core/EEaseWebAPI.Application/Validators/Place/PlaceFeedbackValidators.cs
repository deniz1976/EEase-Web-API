using EEaseWebAPI.Application.Features.Commands.Place.DislikePlace;
using EEaseWebAPI.Application.Features.Commands.Place.LikePlace;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.Place
{
    public class LikePlaceCommandValidator : AbstractValidator<LikePlaceCommandRequest>
    {
        public LikePlaceCommandValidator()
        {
            RuleFor(request => request.GooglePlaceId)
                .NotEmpty().WithMessage(ValidationMessages.GooglePlaceId_Required);

            RuleFor(request => request.PlaceType)
                .NotEmpty().WithMessage(ValidationMessages.PlaceType_Required);
        }
    }

    public class DislikePlaceCommandValidator : AbstractValidator<DislikePlaceCommandRequest>
    {
        public DislikePlaceCommandValidator()
        {
            RuleFor(request => request.GooglePlaceId)
                .NotEmpty().WithMessage(ValidationMessages.GooglePlaceId_Required);

            RuleFor(request => request.PlaceType)
                .NotEmpty().WithMessage(ValidationMessages.PlaceType_Required);

            // Unlike a like, a dislike has to say which route the place is being replaced in.
            RuleFor(request => request.RouteId)
                .NotEmpty().WithMessage(ValidationMessages.RouteId_Required);
        }
    }
}
