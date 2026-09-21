using EEaseWebAPI.Application.Exceptions.Cities;
using EEaseWebAPI.Application.MapEntities.Cities;
using EEaseWebAPI.Domain.Entities.AllWorldCities;

namespace EEaseWebAPI.Persistence.Services.ReferenceData
{
    /// <summary>
    /// What the city list is asked for, once it has been read. It takes the rows and gives
    /// back the answer, so the part with all the rules can be tested without a database:
    /// the table has no primary key, which means no test can put a row into it.
    /// </summary>
    public static class CitySearch
    {
        public const string CapitalCity = "primary";
        public const int MinimumSearchTermLength = 2;

        /// <summary>
        /// The order every search starts from: capitals first, then the biggest places.
        /// </summary>
        public static List<AllWorldCities> Rank(IEnumerable<AllWorldCities> cities) =>
            cities
                .OrderByDescending(city => city.Capital == CapitalCity)
                .ThenByDescending(city => city.Population)
                .ToList();

        public static List<string> NamesOf(IEnumerable<AllWorldCities> cities) =>
            cities
                .Select(NameOf)
                .Where(name => !string.IsNullOrEmpty(name))
                .Distinct()
                .ToList();

        public static List<string> CountriesOf(IEnumerable<AllWorldCities> cities) =>
            cities
                .Select(city => city.Country ?? string.Empty)
                .Where(country => !string.IsNullOrEmpty(country))
                .Distinct()
                .OrderBy(country => country)
                .ToList();

        public static (List<CityDto> Cities, int TotalCount) Search(
            IEnumerable<AllWorldCities> cities,
            string searchTerm,
            string homeCountry,
            int pageSize,
            int pageNumber)
        {
            if (string.IsNullOrEmpty(searchTerm) || searchTerm.Length < MinimumSearchTermLength)
            {
                throw new InvalidSearchTermException();
            }

            // The traveller's own country first, then capitals, then the biggest cities:
            // a search for "san" should not open with a village.
            var matches = cities
                .Where(city => Matches(city, searchTerm))
                .OrderByDescending(city => city.Country == homeCountry)
                .ThenByDescending(city => city.Capital == CapitalCity)
                .ThenByDescending(city => city.Population)
                .ToList();

            var page = matches
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(city => new CityDto
                {
                    CityName = NameOf(city),
                    Country = city.Country ?? string.Empty
                })
                .ToList();

            return (page, matches.Count);
        }

        private static string NameOf(AllWorldCities city) =>
            city.City ?? city.CityAscii ?? string.Empty;

        private static bool Matches(AllWorldCities city, string searchTerm) =>
            city.City?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true ||
            city.CityAscii?.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) == true;
    }
}
