using EEaseWebAPI.Application.Exceptions.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities.Cities;
using EEaseWebAPI.Domain.Entities.AllWorldCities;
using EEaseWebAPI.Persistence.Services.ReferenceData;
using FluentAssertions;
using Xunit;

namespace EEaseWebAPI.UnitTests.ReferenceData
{
    /// <summary>
    /// The city table has no primary key, so no test can put a row into it; the rules that
    /// act on the rows live apart from the reading of them, and this is where they are
    /// checked. <see cref="CityService"/> reads the table and hands the rows here.
    /// </summary>
    public class CitySearchTests
    {
        private const string Turkey = "Turkey";

        private static readonly List<AllWorldCities> Cities = CitySearch.Rank(new[]
        {
            City("Rome", "Italy", capital: true, population: 2_800_000),
            City("Romeoville", "United States", capital: false, population: 39_000),
            City("Milan", "Italy", capital: false, population: 1_400_000),
            City("İzmir", Turkey, capital: false, population: 3_000_000, ascii: "Izmir"),
            City("Ankara", Turkey, capital: true, population: 5_100_000),
            City("Karabük", Turkey, capital: false, population: 9_000_000, ascii: "Karabuk")
        });

        private static AllWorldCities City(
            string name, string country, bool capital, double population, string? ascii = null) =>
            new()
            {
                City = name,
                CityAscii = ascii ?? name,
                Country = country,
                Capital = capital ? "primary" : "admin",
                Population = population
            };

        private static (List<CityDto> Cities, int TotalCount) Search(
            string term, string homeCountry = Turkey, int pageSize = 10, int pageNumber = 1) =>
            CitySearch.Search(Cities, term, homeCountry, pageSize, pageNumber);

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        public void A_search_term_shorter_than_two_characters_is_refused(string searchTerm)
        {
            var act = () => Search(searchTerm);

            act.Should().Throw<InvalidSearchTermException>();
        }

        [Fact]
        public void A_search_matches_anywhere_in_the_name_whatever_the_case()
        {
            var (cities, _) = Search("ROME");

            cities.Select(city => city.CityName).Should().BeEquivalentTo(new[] { "Rome", "Romeoville" });
        }

        [Fact]
        public void A_city_can_be_found_by_its_ascii_spelling()
        {
            Search("Izmir").Cities.Single().CityName.Should().Be("İzmir");
        }

        [Fact]
        public void The_country_the_traveller_lives_in_comes_first()
        {
            Search("an", homeCountry: "Italy").Cities.First().Country.Should().Be("Italy");
        }

        [Fact]
        public void Somebody_with_no_country_of_their_own_gets_the_default_one_first()
        {
            Search("an").Cities.First().Country.Should().Be(Turkey);
        }

        [Fact]
        public void A_capital_outranks_a_bigger_city_of_the_same_country()
        {
            // Karabük is the larger of the two, Ankara is the capital.
            Search("ka").Cities.First().CityName.Should().Be("Ankara");
        }

        [Fact]
        public void A_page_reports_how_many_matches_there_are_in_total()
        {
            var (cities, totalCount) = Search("rome", pageSize: 1);

            cities.Should().ContainSingle();
            totalCount.Should().Be(2);
        }

        [Fact]
        public void The_second_page_carries_on_where_the_first_stopped()
        {
            var first = Search("rome", pageSize: 1, pageNumber: 1).Cities.Single();
            var second = Search("rome", pageSize: 1, pageNumber: 2).Cities.Single();

            second.CityName.Should().NotBe(first.CityName);
        }

        [Fact]
        public void A_search_that_matches_nothing_returns_nothing()
        {
            var (cities, totalCount) = Search("zzz");

            cities.Should().BeEmpty();
            totalCount.Should().Be(0);
        }

        [Fact]
        public void Countries_are_listed_once_each_in_alphabetical_order()
        {
            CitySearch.CountriesOf(Cities).Should().Equal("Italy", Turkey, "United States");
        }

        [Fact]
        public void City_names_are_listed_once_each()
        {
            CitySearch.NamesOf(Cities).Should().HaveCount(6).And.OnlyHaveUniqueItems();
        }

        [Fact]
        public void The_ranking_puts_capitals_before_bigger_places()
        {
            CitySearch.Rank(Cities).First().City.Should().Be("Ankara");
        }
    }
}
