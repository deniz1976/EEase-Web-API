using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.Options;
using EEaseWebAPI.Application.Repositories;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Persistence.Services.Caching;
using EEaseWebAPI.Persistence.Services.ReferenceData;
using FluentAssertions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using NSubstitute;
using Xunit;

namespace EEaseWebAPI.UnitTests.ReferenceData
{
    public class CityServiceTests
    {
        private readonly IAllWorldCitiesRepository _cities = Substitute.For<IAllWorldCitiesRepository>();

        private readonly UserManager<AppUser> _userManager = Substitute.For<UserManager<AppUser>>(
            Substitute.For<IUserStore<AppUser>>(),
            null, null, null, null, null, null, null, null);

        private readonly CityService _service;

        private readonly AppUser _italian = new() { Id = "italian-id", UserName = "marco", Country = "Italy" };
        private readonly AppUser _nomad = new() { Id = "nomad-id", UserName = "nomad" };

        public CityServiceTests()
        {
            _cities.GetAllCitiesAsync().Returns(_ => new List<AllWorldCities>
            {
                City("Rome", "Italy", capital: true, population: 2_800_000),
                City("Romeoville", "United States", capital: false, population: 39_000),
                City("Milan", "Italy", capital: false, population: 1_400_000),
                City("İzmir", "Turkey", capital: false, population: 3_000_000, ascii: "Izmir"),
                City("Ankara", "Turkey", capital: true, population: 5_100_000),
                City("Karabük", "Turkey", capital: false, population: 9_000_000, ascii: "Karabuk")
            });

            _userManager.FindByNameAsync("marco").Returns(_italian);
            _userManager.FindByNameAsync("nomad").Returns(_nomad);

            _service = new CityService(
                _cities,
                new ReferenceDataCache(new MemoryCache(new MemoryCacheOptions()), Options.Create(new CacheOptions())),
                _userManager);
        }

        private static AllWorldCities City(
            string name, string country, bool capital, double population, string? ascii = null) =>
            new()
            {
                city = name,
                city_ascii = ascii ?? name,
                country = country,
                capital = capital ? "primary" : "admin",
                population = population
            };

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        public async Task A_search_term_shorter_than_two_characters_is_refused(string searchTerm)
        {
            await _service.Invoking(service => service.GetCitiesBySearchAsync(searchTerm, 10, 1, null))
                .Should().ThrowAsync<InvalidSearchTermException>();
        }

        [Fact]
        public async Task A_search_matches_anywhere_in_the_name_whatever_the_case()
        {
            var (cities, _) = await _service.GetCitiesBySearchAsync("ROME", 10, 1, null);

            cities.Select(city => city.CityName).Should().BeEquivalentTo(new[] { "Rome", "Romeoville" });
        }

        [Fact]
        public async Task A_city_can_be_found_by_its_ascii_spelling()
        {
            var (cities, _) = await _service.GetCitiesBySearchAsync("Izmir", 10, 1, null);

            cities.Single().CityName.Should().Be("İzmir");
        }

        [Fact]
        public async Task The_country_the_traveller_lives_in_comes_first()
        {
            var (cities, _) = await _service.GetCitiesBySearchAsync("an", 10, 1, "marco");

            cities.First().Country.Should().Be("Italy");
        }

        [Fact]
        public async Task Without_a_user_the_results_favour_the_default_country()
        {
            var (cities, _) = await _service.GetCitiesBySearchAsync("an", 10, 1, null);

            cities.First().Country.Should().Be("Turkey");
        }

        [Fact]
        public async Task A_user_who_never_set_a_country_is_treated_like_a_stranger()
        {
            var (withUser, _) = await _service.GetCitiesBySearchAsync("an", 10, 1, "nomad");
            var (withoutUser, _) = await _service.GetCitiesBySearchAsync("an", 10, 1, null);

            withUser.Should().BeEquivalentTo(withoutUser, options => options.WithStrictOrdering());
        }

        [Fact]
        public async Task A_capital_outranks_a_bigger_city_of_the_same_country()
        {
            // Karabük is the larger of the two, Ankara is the capital.
            var (cities, _) = await _service.GetCitiesBySearchAsync("ka", 10, 1, null);

            cities.First().CityName.Should().Be("Ankara");
        }

        [Fact]
        public async Task A_page_reports_how_many_matches_there_are_in_total()
        {
            var (cities, totalCount) = await _service.GetCitiesBySearchAsync("rome", 1, 1, null);

            cities.Should().ContainSingle();
            totalCount.Should().Be(2);
        }

        [Fact]
        public async Task The_second_page_carries_on_where_the_first_stopped()
        {
            var (first, _) = await _service.GetCitiesBySearchAsync("rome", 1, 1, null);
            var (second, _) = await _service.GetCitiesBySearchAsync("rome", 1, 2, null);

            second.Single().CityName.Should().NotBe(first.Single().CityName);
        }

        [Fact]
        public async Task A_search_that_matches_nothing_returns_nothing()
        {
            var (cities, totalCount) = await _service.GetCitiesBySearchAsync("zzz", 10, 1, null);

            cities.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public async Task Countries_are_listed_once_each_in_alphabetical_order()
        {
            (await _service.GetAllCountries()).Should().Equal("Italy", "Turkey", "United States");
        }

        [Fact]
        public async Task City_names_are_listed_once_each()
        {
            var names = await _service.GetAllCityNames();

            names.Should().HaveCount(6).And.OnlyHaveUniqueItems();
        }

        [Fact]
        public async Task Warming_the_cache_leaves_every_list_usable()
        {
            await _service.InitializeCacheAsync();

            // The three lists used to share a cache key, so warming them up left only the
            // last one cached and the others reading from the database on every request.
            (await _service.GetAllCountries()).Should().NotBeEmpty();
            (await _service.GetAllCityNames()).Should().NotBeEmpty();
            (await _service.GetCitiesBySearchAsync("rome", 10, 1, null)).Cities.Should().NotBeEmpty();
        }
    }
}
