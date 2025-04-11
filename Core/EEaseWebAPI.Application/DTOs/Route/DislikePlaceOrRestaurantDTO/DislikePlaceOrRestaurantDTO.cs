using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.DTOs.Route.DislikePlaceOrRestaurantDTO
{
    public class DislikePlaceOrRestaurantDTO
    {
        public string GooglePlaceId { get; set; }
        public string PlaceType { get; set; }

        public string? UserFeedback {  get; set; }

        public int? DislikeType { get; set; }
    }
}
