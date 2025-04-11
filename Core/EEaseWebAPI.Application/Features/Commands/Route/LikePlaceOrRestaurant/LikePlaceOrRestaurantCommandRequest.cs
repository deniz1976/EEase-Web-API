using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant
{
    public class LikePlaceOrRestaurantCommandRequest : IRequest<LikePlaceOrRestaurantCommandResponse>
    {
        /// <summary>
        /// The Google Place ID of the place/restaurant
        /// </summary>
        public string GooglePlaceId { get; set; }

        /// <summary>
        /// The type of place (TravelAccommodation, Breakfast, Lunch, Dinner, PlaceAfterDinner, Place)
        /// </summary>
        public string PlaceType { get; set; }

        /// <summary>
        /// The username of the user liking the place (set automatically by the controller)
        /// </summary>
        public string Username { get; set; }
    }
} 