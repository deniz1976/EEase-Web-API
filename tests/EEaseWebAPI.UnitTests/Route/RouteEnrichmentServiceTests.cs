using AutoMapper;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Mappings;
using EEaseWebAPI.Domain.Entities.Route;
using EEaseWebAPI.Persistence.Services.Route;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.Route
{
    public class RouteEnrichmentServiceTests
    {
        private static IMapper CreateMapper() =>
            new MapperConfiguration(
                configuration => configuration.AddProfile<RouteEnrichmentProfile>(),
                NullLoggerFactory.Instance)
                .CreateMapper();

        private static StandardRoute CreateRoute(int dayCount)
        {
            var route = new StandardRoute
            {
                Id = Guid.NewGuid(),
                City = "Lisbon",
                Currency = "EUR",
                TravelDays = new List<TravelDay>()
            };

            for (var day = 0; day < dayCount; day++)
            {
                route.TravelDays.Add(new TravelDay
                {
                    Accomodation = new TravelAccomodation(),
                    Breakfast = new Breakfast(),
                    Lunch = new Lunch(),
                    Dinner = new Dinner(),
                    FirstPlace = new Place(),
                    SecondPlace = new Place(),
                    ThirdPlace = new Place(),
                    PlaceAfterDinner = new PlaceAfterDinner()
                });
            }

            return route;
        }

        private static (RouteEnrichmentService Service, IGeminiApiClient Client) CreateService(string response)
        {
            var client = Substitute.For<IGeminiApiClient>();
            client.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns(response);

            var service = new RouteEnrichmentService(
                client,
                CreateMapper(),
                NullLogger<RouteEnrichmentService>.Instance);

            return (service, client);
        }

        [Fact]
        public void The_mapping_configuration_is_valid()
        {
            var configuration = new MapperConfiguration(
                config => config.AddProfile<RouteEnrichmentProfile>(),
                NullLoggerFactory.Instance);

            configuration.AssertConfigurationIsValid();
        }

        [Fact]
        public async Task Prices_descriptions_star_and_weather_land_on_the_route()
        {
            var (service, _) = CreateService("""
            {
              "approxPrices": ["EUR 100", "EUR 120"],
              "star": "4",
              "dayDescriptions": ["First day", "Second day"],
              "weathers": [
                [
                  {"Degree": 20, "Description": "Sunny", "Warning": "Sunscreen", "Date": "2026-09-10"},
                  {"Degree": 21, "Description": "Clear", "Warning": null, "Date": "2026-09-10"},
                  {"Degree": 22, "Description": "Warm", "Warning": null, "Date": "2026-09-10"},
                  {"Degree": 23, "Description": "Hot", "Warning": null, "Date": "2026-09-10"},
                  {"Degree": 22, "Description": "Mild", "Warning": null, "Date": "2026-09-10"},
                  {"Degree": 19, "Description": "Cooling", "Warning": null, "Date": "2026-09-10"},
                  {"Degree": 17, "Description": "Cool", "Warning": "Take a jacket", "Date": "2026-09-10"}
                ],
                [
                  {"Degree": 15, "Description": "Rainy", "Warning": "Umbrella", "Date": "2026-09-11"}
                ]
              ]
            }
            """);

            var route = CreateRoute(2);

            var applied = await service.ApplyAsync(route, new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 11));

            applied.Should().BeTrue();

            route.TravelDays[0].ApproxPrice.Should().Be("EUR 100");
            route.TravelDays[1].DayDescription.Should().Be("Second day");
            route.TravelDays[0].Accomodation!.Star.Should().Be("4");

            route.TravelDays[0].Breakfast!.Weather!.Degree.Should().Be(20);
            route.TravelDays[0].FirstPlace!.Weather!.Description.Should().Be("Clear");
            route.TravelDays[0].Lunch!.Weather!.Description.Should().Be("Warm");
            route.TravelDays[0].SecondPlace!.Weather!.Description.Should().Be("Hot");
            route.TravelDays[0].ThirdPlace!.Weather!.Description.Should().Be("Mild");
            route.TravelDays[0].Dinner!.Weather!.Description.Should().Be("Cooling");
            route.TravelDays[0].PlaceAfterDinner!.Weather!.Warning.Should().Be("Take a jacket");
            route.TravelDays[0].Breakfast!.Weather!.Date.Should().Be(new DateOnly(2026, 9, 10));
        }

        [Fact]
        public async Task A_response_that_is_shorter_than_the_route_does_not_throw()
        {
            var (service, _) = CreateService("""
            {
              "approxPrices": ["EUR 100"],
              "star": "3",
              "dayDescriptions": ["Only the first day"],
              "weathers": [[{"Degree": 20, "Description": "Sunny", "Date": "2026-09-10"}]]
            }
            """);

            var route = CreateRoute(3);

            var applied = await service.ApplyAsync(route, new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 12));

            applied.Should().BeTrue();

            route.TravelDays[0].ApproxPrice.Should().Be("EUR 100");
            route.TravelDays[2].ApproxPrice.Should().BeNull();
            route.TravelDays[2].DayDescription.Should().BeNull();
            route.TravelDays[0].Breakfast!.Weather.Should().NotBeNull();
            route.TravelDays[0].Lunch!.Weather.Should().BeNull();
            route.TravelDays[2].Breakfast!.Weather.Should().BeNull();
        }

        [Fact]
        public async Task A_response_wrapped_in_a_code_fence_is_still_read()
        {
            var (service, _) = CreateService("```json\n{\"approxPrices\": [\"EUR 90\"], \"star\": \"5\"}\n```");

            var route = CreateRoute(1);

            var applied = await service.ApplyAsync(route, null, null);

            applied.Should().BeTrue();
            route.TravelDays[0].ApproxPrice.Should().Be("EUR 90");
            route.TravelDays[0].Accomodation!.Star.Should().Be("5");
        }

        [Fact]
        public async Task Unusable_output_leaves_the_route_untouched_instead_of_failing()
        {
            var (service, _) = CreateService("not json at all");

            var route = CreateRoute(2);
            route.TravelDays[0].ApproxPrice = "EUR 50";

            var applied = await service.ApplyAsync(route, null, null);

            applied.Should().BeFalse();
            route.TravelDays[0].ApproxPrice.Should().Be("EUR 50");
        }

        [Fact]
        public async Task A_failing_provider_does_not_break_route_creation()
        {
            var client = Substitute.For<IGeminiApiClient>();
            client.GenerateContentAsync(Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>())
                .Returns<string>(_ => throw new InvalidOperationException("provider down"));

            var service = new RouteEnrichmentService(
                client,
                CreateMapper(),
                NullLogger<RouteEnrichmentService>.Instance);

            var applied = await service.ApplyAsync(CreateRoute(1), null, null);

            applied.Should().BeFalse();
        }

        [Fact]
        public async Task An_empty_route_is_not_sent_to_the_provider()
        {
            var (service, client) = CreateService("{}");

            (await service.ApplyAsync(null, null, null)).Should().BeFalse();
            (await service.ApplyAsync(new StandardRoute { TravelDays = new List<TravelDay>() }, null, null))
                .Should().BeFalse();

            await client.DidNotReceive().GenerateContentAsync(
                Arg.Any<string>(), Arg.Any<bool>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task The_prompt_asks_for_json_and_names_the_places()
        {
            var (service, client) = CreateService("{}");

            var route = CreateRoute(1);
            route.TravelDays[0].Breakfast!.DisplayName = new DisplayName { Text = "Cafe Brasileira" };

            await service.ApplyAsync(route, new DateOnly(2026, 9, 10), new DateOnly(2026, 9, 10));

            await client.Received(1).GenerateContentAsync(
                Arg.Is<string>(prompt => prompt.Contains("Cafe Brasileira") && prompt.Contains("Lisbon")),
                true,
                Arg.Any<CancellationToken>());
        }
    }
}
