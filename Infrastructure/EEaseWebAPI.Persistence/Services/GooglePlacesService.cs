using System.Text;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.GooglePlaces;
using EEaseWebAPI.Application.Exceptions;
using EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto;
using EEaseWebAPI.Application.Options;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;

namespace EEaseWebAPI.Persistence.Services
{
    public sealed class GooglePlacesService : IGooglePlacesService
    {
        private const string PlaceDetailsFieldMask =
            "nationalPhoneNumber,formattedAddress,rating,googleMapsUri,websiteUri,goodForChildren," +
            "restroom,primaryType,location,regularOpeningHours,displayName,photos,paymentOptions," +
            "priceLevel,menuForChildren,liveMusic,outdoorSeating,shortFormattedAddress," +
            "servesVegetarianFood,servesBrunch,reservable,takeout,delivery,curbsidePickup," +
            "servesBeer,servesWine,servesCocktails,internationalPhoneNumber";

        private static readonly JsonSerializerSettings JsonOptions = new()
        {
            MissingMemberHandling = MissingMemberHandling.Ignore,
            NullValueHandling = NullValueHandling.Ignore
        };

        private readonly HttpClient _httpClient;
        private readonly GooglePlacesOptions _options;

        public GooglePlacesService(HttpClient httpClient, IOptions<GooglePlacesOptions> options)
        {
            _httpClient = httpClient;
            _options = options.Value;
        }

        public async Task<PlaceSearchResponse> SearchPlacesAsync(string query, string? type = null)
        {
            using var request = CreateRequest(HttpMethod.Post, "v1/places:searchText");

            request.Content = new StringContent(
                JsonConvert.SerializeObject(new { textQuery = query }),
                Encoding.UTF8,
                "application/json");

            request.Headers.Add("X-Goog-FieldMask", "places.id");

            var body = await SendAsync(request);

            return JsonConvert.DeserializeObject<PlaceSearchResponse>(body, JsonOptions)
                   ?? new PlaceSearchResponse();
        }

        public async Task<string> GetPlaceDetailsAsync(string placeId)
        {
            using var request = CreateRequest(HttpMethod.Get, $"v1/places/{placeId}");
            request.Headers.Add("X-Goog-FieldMask", PlaceDetailsFieldMask);

            return await SendAsync(request);
        }

        public async Task<GetRouteComponentPhotoCommandResponseBody> GetPlacePhotosAsync(
            string photoName,
            int maxWidth = 400,
            int maxHeight = 400)
        {
            var url = $"v1/{photoName}/media" +
                      $"?maxHeightPx={maxHeight}&maxWidthPx={maxWidth}&skipHttpRedirect=true";

            using var request = CreateRequest(HttpMethod.Get, url);

            var body = await SendAsync(request);

            return JsonConvert.DeserializeObject<GetRouteComponentPhotoCommandResponseBody>(body, JsonOptions)
                   ?? new GetRouteComponentPhotoCommandResponseBody();
        }

        private HttpRequestMessage CreateRequest(HttpMethod method, string url)
        {
            if (string.IsNullOrWhiteSpace(_options.ApiKey))
            {
                throw new ExternalServiceNotConfiguredException("Google Places API", "GooglePlaces:ApiKey");
            }

            var request = new HttpRequestMessage(method, url);
            request.Headers.Add("X-Goog-Api-Key", _options.ApiKey);

            return request;
        }

        private async Task<string> SendAsync(HttpRequestMessage request)
        {
            using var response = await _httpClient.SendAsync(request);
            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                var detail = body.Length <= 500 ? body : body[..500] + "…";

                throw new HttpRequestException(
                    $"The Google Places API returned {(int)response.StatusCode}. Response: {detail}",
                    inner: null,
                    statusCode: response.StatusCode);
            }

            return body;
        }
    }
}
