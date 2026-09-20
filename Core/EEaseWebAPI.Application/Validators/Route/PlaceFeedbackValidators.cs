using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.Resources;
using FluentValidation;

namespace EEaseWebAPI.Application.Validators.Route
{
    /// <summary>
    /// Both of these say what the traveller thought of one place on one route. The controller
    /// used to check the same fields by hand and answer a bare string, which is the one shape
    /// of error the rest of the API does not speak.
    /// </summary>
    public class LikePlaceOrRestaurantCommandValidator : AbstractValidator<LikePlaceOrRestaurantCommandRequest>
    {
        public LikePlaceOrRestaurantCommandValidator()
        {
            RuleFor(request => request.GooglePlaceId)
                .NotEmpty().WithMessage(ValidationMessages.GooglePlaceId_Required);

            RuleFor(request => request.PlaceType)
                .NotEmpty().WithMessage(ValidationMessages.PlaceType_Required);
        }
    }

    public class DislikePlaceOrRestaurantCommandValidator : AbstractValidator<DislikePlaceOrRestaurantCommandRequest>
    {
        public DislikePlaceOrRestaurantCommandValidator()
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
