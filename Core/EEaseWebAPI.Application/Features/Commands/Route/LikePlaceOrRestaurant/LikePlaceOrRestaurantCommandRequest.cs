using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant
{
    public class LikePlaceOrRestaurantCommandRequest : IRequest<LikePlaceOrRestaurantCommandResponse>
    {
        public string GooglePlaceId { get; set; }

        public string PlaceType { get; set; }

        public string Username { get; set; }
    }
}
