using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant
{
    public class LikePlaceOrRestaurantCommandRequest : IRequest<LikePlaceOrRestaurantCommandResponse>
    {
        public string GooglePlaceId { get; set; } = string.Empty;

        public string PlaceType { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;
    }
}
