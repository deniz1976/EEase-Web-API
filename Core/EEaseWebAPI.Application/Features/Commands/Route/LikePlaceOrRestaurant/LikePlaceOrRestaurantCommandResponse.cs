using EEaseWebAPI.Application.DTOs;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant
{
    public class LikePlaceOrRestaurantCommandResponse
    {
        public MapEntities.Header? Header { get; set; }
        public LikePlaceOrRestaurantCommandResponseBody? Body { get; set; }
    }

    public class LikePlaceOrRestaurantCommandResponseBody
    {
        public bool IsPreferenceUpdated { get; set; }
        public string Message { get; set; }
    }
} 