using System.ComponentModel.DataAnnotations.Schema;

namespace EEaseWebAPI.Domain.Entities.AllWorldCities
{
    public class AllWorldCities
    {
        [Column("city")]
        public string? City { get; set; }

        [Column("city_ascii")]
        public string? CityAscii { get; set; }

        [Column("lat")]
        public double? Latitude { get; set; }

        [Column("lng")]
        public double? Longitude { get; set; }

        [Column("country")]
        public string? Country { get; set; }

        [Column("iso2")]
        public string? Iso2 { get; set; }

        [Column("iso3")]
        public string? Iso3 { get; set; }

        [Column("admin_name")]
        public string? AdminName { get; set; }

        [Column("capital")]
        public string? Capital { get; set; }

        [Column("population")]
        public double? Population { get; set; }

        [Column("id")]
        public int? Id { get; set; }
    }
}
