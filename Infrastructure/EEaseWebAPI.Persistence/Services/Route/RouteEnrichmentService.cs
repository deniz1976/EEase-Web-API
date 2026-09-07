using System.Text.Json;
using AutoMapper;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route.Enrichment;
using EEaseWebAPI.Domain.Entities.Route;
using Microsoft.Extensions.Logging;

namespace EEaseWebAPI.Persistence.Services.Route
{
    public sealed class RouteEnrichmentService : IRouteEnrichmentService
    {
        private const int WeatherEntriesPerDay = 7;

        private static readonly JsonSerializerOptions SerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly IGeminiApiClient _geminiApiClient;
        private readonly IMapper _mapper;
        private readonly ILogger<RouteEnrichmentService> _logger;

        public RouteEnrichmentService(
            IGeminiApiClient geminiApiClient,
            IMapper mapper,
            ILogger<RouteEnrichmentService> logger)
        {
            _geminiApiClient = geminiApiClient;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<bool> ApplyAsync(
            StandardRoute? route,
            DateOnly? startDate,
            DateOnly? endDate,
            CancellationToken cancellationToken = default)
        {
            if (route?.TravelDays == null || route.TravelDays.Count == 0)
            {
                return false;
            }

            var response = await RequestAsync(route, startDate, endDate, cancellationToken);
            if (response == null)
            {
                return false;
            }

            var days = Normalize(response, route.TravelDays.Count);

            for (var index = 0; index < route.TravelDays.Count; index++)
            {
                Apply(route.TravelDays[index], days[index], response.Star);
            }

            return true;
        }

        private void Apply(TravelDay? day, RouteDayEnrichment enrichment, string? star)
        {
            if (day == null)
            {
                return;
            }

            _mapper.Map(enrichment, day);

            if (day.Accomodation != null && !string.IsNullOrWhiteSpace(star))
            {
                day.Accomodation.Star = star;
            }

            day.Breakfast = ApplyWeather(day.Breakfast, enrichment, RouteDaySlot.Breakfast);
            day.FirstPlace = ApplyWeather(day.FirstPlace, enrichment, RouteDaySlot.FirstPlace);
            day.Lunch = ApplyWeather(day.Lunch, enrichment, RouteDaySlot.Lunch);
            day.SecondPlace = ApplyWeather(day.SecondPlace, enrichment, RouteDaySlot.SecondPlace);
            day.ThirdPlace = ApplyWeather(day.ThirdPlace, enrichment, RouteDaySlot.ThirdPlace);
            day.Dinner = ApplyWeather(day.Dinner, enrichment, RouteDaySlot.Dinner);
            day.PlaceAfterDinner = ApplyWeather(day.PlaceAfterDinner, enrichment, RouteDaySlot.AfterDinner);
        }

        private T? ApplyWeather<T>(T? target, RouteDayEnrichment enrichment, RouteDaySlot slot)
            where T : class, IHasWeather
        {
            if (target == null)
            {
                return null;
            }

            var forecast = enrichment.WeatherAt(slot);
            if (forecast != null)
            {
                target.Weather = _mapper.Map<Weather>(forecast);
            }

            return target;
        }

        private static IReadOnlyList<RouteDayEnrichment> Normalize(RouteEnrichmentResponse response, int dayCount)
        {
            var days = new List<RouteDayEnrichment>(dayCount);

            for (var index = 0; index < dayCount; index++)
            {
                days.Add(new RouteDayEnrichment
                {
                    ApproxPrice = ElementAtOrDefault(response.ApproxPrices, index),
                    DayDescription = ElementAtOrDefault(response.DayDescriptions, index),
                    Weather = ElementAtOrDefault(response.Weathers, index)?.Cast<WeatherForecastDto?>().ToArray()
                        ?? Array.Empty<WeatherForecastDto?>()
                });
            }

            return days;
        }

        private static T? ElementAtOrDefault<T>(IReadOnlyList<T>? source, int index) =>
            source != null && index >= 0 && index < source.Count ? source[index] : default;

        private async Task<RouteEnrichmentResponse?> RequestAsync(
            StandardRoute route,
            DateOnly? startDate,
            DateOnly? endDate,
            CancellationToken cancellationToken)
        {
            try
            {
                var prompt = BuildPrompt(route, startDate, endDate);
                var raw = await _geminiApiClient.GenerateContentAsync(prompt, expectJson: true, cancellationToken);

                return Parse(raw);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (Exception exception)
            {
                _logger.LogWarning(
                    exception,
                    "Could not enrich route {RouteId}; it is saved without prices, descriptions and weather.",
                    route.Id);

                return null;
            }
        }

        private RouteEnrichmentResponse? Parse(string raw)
        {
            var json = raw.Replace("```json", string.Empty).Replace("```", string.Empty).Trim();

            var response = JsonSerializer.Deserialize<RouteEnrichmentResponse>(json, SerializerOptions);

            if (response == null)
            {
                _logger.LogWarning("The enrichment response could not be read as JSON.");
            }

            return response;
        }

        private static string BuildPrompt(StandardRoute route, DateOnly? startDate, DateOnly? endDate)
        {
            var currency = route.Currency;
            var destination = route.City;
            var dayCount = route.TravelDays!.Count;
            var hotelName = route.TravelDays[^1]?.Accomodation?.DisplayName?.Text;

            var placeNames = route.TravelDays
                .Select(day => new List<string>
                {
                    day?.Accomodation?.DisplayName?.Text ?? "Hotel",
                    day?.Breakfast?.DisplayName?.Text ?? "Breakfast Place",
                    day?.Lunch?.DisplayName?.Text ?? "Lunch Place",
                    day?.Dinner?.DisplayName?.Text ?? "Dinner Place",
                    day?.FirstPlace?.DisplayName?.Text ?? "First Attraction",
                    day?.SecondPlace?.DisplayName?.Text ?? "Second Attraction",
                    day?.ThirdPlace?.DisplayName?.Text ?? "Third Attraction",
                    day?.PlaceAfterDinner?.DisplayName?.Text ?? "Evening Venue"
                })
                .ToList();

            var placeNamesJson = JsonSerializer.Serialize(placeNames);

            return $@"You are planning a {dayCount}-day trip to {destination}, from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}.

Return JSON with exactly these fields:

1. 'approxPrices': an array of EXACTLY {dayCount} strings, one per day, formatted as ""{currency} 100"" (currency, space, amount).
   Estimate a realistic daily total by adding up: the accommodation, all three meals at the listed restaurants,
   entrance fees for the three attractions, transport between them, the evening venue and incidentals.
   Take the quality and location of each named place into account.

2. 'star': the star rating of the accommodation '{hotelName}', as a string.

3. 'dayDescriptions': an array of EXACTLY {dayCount} strings. Each is a 3-4 sentence, engaging description of that day,
   built from the place names below.

4. 'weathers': an array of EXACTLY {dayCount} arrays. Each inner array holds EXACTLY {WeatherEntriesPerDay} forecasts, in this order:
   09:00, 11:00, 13:00, 15:00, 17:00, 19:00, 21:00.
   Each forecast is an object with 'Degree' (number), 'Description' (string), 'Warning' (string) and
   'Date' (string, yyyy-MM-dd only, no time).

The array lengths above are mandatory. Do not return fewer entries than asked.

Place names per day:
{placeNamesJson}";
        }
    }
}
