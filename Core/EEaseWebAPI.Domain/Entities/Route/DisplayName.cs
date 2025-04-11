using EEaseWebAPI.Domain.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class DisplayName : BaseEntity
    {
        [JsonPropertyName("text")]
        public string? Text { get; set; }

        [JsonPropertyName("languageCode")]
        public string? LangugageCode { get; set; }

        public Guid? BaseRestaurantPlaceEntityId { get; set; }

        public Guid? BaseTravelPlaceEntityId { get; set; }
    }
}
