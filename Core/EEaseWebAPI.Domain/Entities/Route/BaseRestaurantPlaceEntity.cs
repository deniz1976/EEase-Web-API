using EEaseWebAPI.Domain.Entities.Common;
using EEaseWebAPI.Domain.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EEaseWebAPI.Domain.Entities.Route
{
    public class BaseRestaurantPlaceEntity : BaseEntity
    {
        [JsonPropertyName("nationalPhoneNumber")]
        public string? NationalPhoneNumber { get; set; } //
        [JsonPropertyName("formattedAddress")]
        public string? FormattedAddress { get; set; } //
        [JsonPropertyName("rating")]
        public double? Rating { get; set; } //
        [JsonPropertyName("googleMapsUri")]
        public string? GoogleMapsUri { get; set; } //
        [JsonPropertyName("websiteUri")]
        public string? WebsiteUri { get; set; } //
        [JsonPropertyName("primaryType")]
        public string? PrimaryType { get; set; } // 
        [JsonPropertyName("id")]
        public string? GoogleId { get; set; } // 
        [JsonPropertyName("reservable")]
        public bool? Reservable { get; set; }
        [JsonPropertyName("servesBrunch")]
        public bool? ServesBrunch { get; set; }
        [JsonPropertyName("servesVegetarianFood")]
        public bool? ServesVegetarianFood { get; set; }
        [JsonPropertyName("shortFormattedAddress")]
        public string? ShortFormattedAddress { get; set; }
        [JsonPropertyName("outdoorSeating")]
        public bool? OutdoorSeating { get; set; }
        [JsonPropertyName("liveMusic")]
        public bool? LiveMusic { get; set; }
        [JsonPropertyName("menuForChildren")]
        public bool? MenuForChildren { get; set; }
        [JsonPropertyName("restroom")]
        public bool? Restroom { get; set; } //
        [JsonPropertyName("goodForGroups")]
        public bool? GoodForGroups { get; set; }
        [JsonPropertyName("location")]
        public Location? Location { get; set; } //
        [JsonPropertyName("regularOpeningHours")]
        public RegularOpeningHours? RegularOpeningHours { get; set; } //

        public PRICE_LEVEL? _PRICE_LEVEL { get; set; }

        [JsonPropertyName("displayName")]
        public DisplayName? DisplayName { get; set; } //

        [JsonPropertyName("paymentOptions")]
        public PaymentOptions? PaymentOptions { get; set; }//
        [JsonPropertyName("photos")]
        public List<Photos>? Photos { get; set; }//

        [JsonPropertyName("weather")]
        public Weather? Weather { get; set; } //

        public UserFoodPreferences? UserFoodPreferences { get; set; } = null;
        public UserPersonalization? UserPersonalization { get; set; } = null;

        public UserAccommodationPreferences? UserAccommodationPreferences { get; set; } = null;

        public string? UserFoodPreference { get; set; }




    }
}
