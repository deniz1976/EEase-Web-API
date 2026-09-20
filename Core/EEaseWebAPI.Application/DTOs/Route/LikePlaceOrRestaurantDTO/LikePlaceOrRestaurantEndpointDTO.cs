using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.Route.LikePlaceOrRestaurantDTO
{
    public class LikePlaceOrRestaurantEndpointDTO
    {
        public string GooglePlaceId { get; set; } = string.Empty;
        public string PlaceType { get; set; } = string.Empty;
    }
}
