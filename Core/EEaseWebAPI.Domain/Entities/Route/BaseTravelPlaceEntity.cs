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
    public class BaseTravelPlaceEntity : BaseEntity
    {
        [JsonPropertyName("nationalPhoneNumber")]
        public string? NationalPhoneNumber { get; set; } //

        [JsonPropertyName("formattedAddress")]
        public string? FormattedAddress { get; set; }//


        [JsonPropertyName("rating")]
        public double? Rating { get; set; }//


        [JsonPropertyName("googleMapsUri")]
        public string? GoogleMapsUri { get; set; }//


        [JsonPropertyName("websiteUri")]
        public string? WebsiteUri { get; set; }//


        [JsonPropertyName("goodForChildren")]
        public bool? GoodForChildren { get; set; } // 


        [JsonPropertyName("restroom")]
        public bool? Restroom { get; set; } //


        [JsonPropertyName("primaryType")]
        public string? PrimaryType { get; set; } //


        [JsonPropertyName("id")]
        public string? GoogleId { get; set; } //


        [JsonPropertyName("location")]
        public Location? Location { get; set; } //


        [JsonPropertyName("regularOpeningHours")]
        public RegularOpeningHours? RegularOpeningHours { get; set; }//


        [JsonPropertyName("displayName")]
        public DisplayName? DisplayName { get; set; }//


        [JsonPropertyName("photos")]
        public List<Photos>? Photos { get; set; }//


        [JsonPropertyName("paymentOptions")]
        public PaymentOptions? PaymentOptions { get; set; }//


        public PRICE_LEVEL? _PRICE_LEVEL { get; set; }//

        public UserFoodPreferences? UserFoodPreferences { get; set; } = null;
        public UserPersonalization? UserPersonalization { get; set; } = null;

        public UserAccommodationPreferences? UserAccommodationPreferences { get; set; } = null;

        public string? UserPersonalizationPref { get; set; }

    }
}
