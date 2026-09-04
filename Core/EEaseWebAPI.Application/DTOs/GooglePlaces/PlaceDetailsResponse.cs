using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EEaseWebAPI.Application.DTOs.GooglePlaces
{
    public class PlaceDetailsResponse
    {
        [JsonPropertyName("result")]
        public PlaceDetail Result { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; }
    }

    public class PlaceDetail
    {
        [JsonPropertyName("place_id")]
        public string PlaceId { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("formatted_address")]
        public string FormattedAddress { get; set; }

        [JsonPropertyName("geometry")]
        public Geometry Geometry { get; set; }

        [JsonPropertyName("rating")]
        public double? Rating { get; set; }

        [JsonPropertyName("user_ratings_total")]
        public int? UserRatingsTotal { get; set; }

        [JsonPropertyName("photos")]
        public List<Photo> Photos { get; set; } = new List<Photo>();

        [JsonPropertyName("types")]
        public List<string> Types { get; set; } = new List<string>();
    }

    public class Geometry
    {
        [JsonPropertyName("location")]
        public Location Location { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("lat")]
        public double Latitude { get; set; }

        [JsonPropertyName("lng")]
        public double Longitude { get; set; }
    }

    public class Photo
    {
        [JsonPropertyName("photo_reference")]
        public string PhotoReference { get; set; }

        [JsonPropertyName("height")]
        public int Height { get; set; }

        [JsonPropertyName("width")]
        public int Width { get; set; }

        [JsonPropertyName("html_attributions")]
        public List<string> HtmlAttributions { get; set; } = new List<string>();
    }
} 