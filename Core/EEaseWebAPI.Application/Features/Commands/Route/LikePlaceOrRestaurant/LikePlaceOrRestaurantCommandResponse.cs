using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.DTOs;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant
{
    public class LikePlaceOrRestaurantCommandResponse : ApiResponse<LikePlaceOrRestaurantCommandResponseBody>
    {
    }

    public class LikePlaceOrRestaurantCommandResponseBody
    {
        public bool IsPreferenceUpdated { get; set; }
        public string Message { get; set; }
    }
}
