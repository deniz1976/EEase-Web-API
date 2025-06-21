using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace EEaseWebAPI.Persistence.Services
{
    public class GooglePlacesService : IGooglePlacesService
    {
        private readonly HttpClient _httpClient;
        private readonly string? _apiKey;
        private readonly JsonSerializerSettings _jsonOptions;

        public GooglePlacesService(IConfiguration configuration, HttpClient httpClient)
        {
            _httpClient = httpClient;
            _apiKey = configuration["GooglePlaces:ApiKey"];
            _httpClient.BaseAddress = new Uri("https://maps.googleapis.com/maps/api/place/");
            _jsonOptions = new JsonSerializerSettings
            {
                MissingMemberHandling = MissingMemberHandling.Ignore,
                NullValueHandling = NullValueHandling.Ignore
            };
        }

        public async Task<PlaceSearchResponse> SearchPlacesAsync(string query, string? type = null)
        {
            var request = new
            {
                textQuery = query
            };

            string jsonRequest = JsonConvert.SerializeObject(request);

            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri("https://places.googleapis.com/v1/places:searchText"),
                Content = new StringContent(jsonRequest, Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Add("X-Goog-Api-Key", _apiKey);
            httpRequest.Headers.Add("X-Goog-FieldMask", "places.id");

            HttpResponseMessage response = await _httpClient.SendAsync(httpRequest);

            response.EnsureSuccessStatusCode();
            string responseBody = await response.Content.ReadAsStringAsync();

            PlaceSearchResponse? list = JsonConvert.DeserializeObject<PlaceSearchResponse>(responseBody);
            return list;
        }

        

        public async Task<string> GetPlaceDetailsAsync(string placeId)
        {
            var httpRequest = new HttpRequestMessage
            {
                Method = HttpMethod.Get,
                RequestUri = new Uri($"https://places.googleapis.com/v1/places/{placeId}"),
            };

            httpRequest.Headers.Add("X-Goog-Api-Key", _apiKey);
            httpRequest.Headers.Add("X-Goog-FieldMask", "nationalPhoneNumber,formattedAddress,rating,googleMapsUri,websiteUri,goodForChildren,restroom,primaryType,location,regularOpeningHours,displayName,photos,paymentOptions,priceLevel,goodForChildren,menuForChildren,liveMusic,outdoorSeating,shortFormattedAddress,servesVegetarianFood,servesBrunch,reservable,takeout,delivery,curbsidePickup,servesBeer,servesWine,servesCocktails,internationalPhoneNumber");
            //httpRequest.Headers.Add("X-Goog-FieldMask", "*");
            //httpRequest.Headers.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json")); 

            var response = await _httpClient.SendAsync(httpRequest);
            response.EnsureSuccessStatusCode();

            string responseBody = await response.Content.ReadAsStringAsync();

            return responseBody;
        }


        public async Task<GetRouteComponentPhotoCommandResponseBody> GetPlacePhotosAsync(string photoName,int maxWidth = 400,int maxHeight = 400)
        {
            string url = $"https://places.googleapis.com/v1/{photoName}/media?maxHeightPx={maxHeight}&maxWidthPx={maxWidth}&key={_apiKey}&skipHttpRedirect=true";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            GetRouteComponentPhotoCommandResponseBody? photo = JsonConvert.DeserializeObject<GetRouteComponentPhotoCommandResponseBody>(content);
            return photo;
        }
    }
} 