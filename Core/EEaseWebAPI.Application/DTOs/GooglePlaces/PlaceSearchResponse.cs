using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace EEaseWebAPI.Application.DTOs.GooglePlaces
{
    public class Place
    {
        [JsonProperty("id")]
        public string? Id { get; set; }

        [JsonProperty("types")]
        public List<string>? Types { get; set; }
    }

    public class PlaceSearchResponse
    {
        [JsonProperty("places")]
        public List<Place>? Places { get; set; }
    }


} 