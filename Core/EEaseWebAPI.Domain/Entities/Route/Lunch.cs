using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class Lunch : BaseRestaurantPlaceEntity
    {
        [JsonPropertyName("servesBeer")]
        public bool? ServesBeer { get; set; }

        [JsonPropertyName("servesWine")]
        public bool? ServesWine { get; set; }
    }
}
