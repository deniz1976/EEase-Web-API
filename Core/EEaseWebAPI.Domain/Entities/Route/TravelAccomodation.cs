using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class TravelAccomodation : BaseTravelPlaceEntity
    {
        [JsonPropertyName("star")]
        public string? Star { get; set; }

        [JsonPropertyName("internationalPhoneNumber")]
        public string? InternationalPhoneNumber { get; set; }

        public string? UserAccomodationPreference { get; set; }

        

        
    }
}
