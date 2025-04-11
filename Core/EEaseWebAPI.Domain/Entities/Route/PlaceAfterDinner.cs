using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class PlaceAfterDinner : BaseRestaurantPlaceEntity
    {
        [JsonPropertyName("takeout")]
        public bool? Takeout { get; set; }
        [JsonPropertyName("delivery")]
        public bool? Delivery { get; set; }
        [JsonPropertyName("curbsidePickup")]
        public bool? CurbsidePickup { get; set; }
        [JsonPropertyName("servesBeer")]
        public bool? ServesBeer { get; set; }
        [JsonPropertyName("servesWine")]
        public bool? ServesWine { get; set; }
        [JsonPropertyName("servesCocktails")]
        public bool? ServesCocktails { get; set; }
        [JsonPropertyName("goodForChildren")]
        public bool? GoodForChildren { get; set; }

    }
}
