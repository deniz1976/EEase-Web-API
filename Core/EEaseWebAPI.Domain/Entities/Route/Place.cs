using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class Place : BaseTravelPlaceEntity
    {
        [JsonPropertyName("weather")]
        public Weather? Weather { get; set; }

        
    }
}
