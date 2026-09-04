using System.ComponentModel.DataAnnotations;

namespace EEaseWebAPI.Domain.Entities.AllWorldCities
{
    public class AllWorldCities
    {
        public string? city { get; set; }

        public string? city_ascii { get; set; }

        public double? lat { get; set; }

        public double? lng { get; set; }

        public string? country { get; set; }

        public string? iso2 { get; set; }

        public string? iso3 { get; set; }

        public string? admin_name { get; set; }

        public string? capital { get; set; }

        public double? population { get; set; }

        public int? id { get; set; }
    }
}
