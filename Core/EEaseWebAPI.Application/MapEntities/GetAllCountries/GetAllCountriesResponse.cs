using System.Collections.Generic;

namespace EEaseWebAPI.Application.MapEntities.GetAllCountries
{
    public class GetAllCountriesResponse
    {
        public Header? Header { get; set; }
        public GetAllCountriesBody? Body { get; set; }
    }

    public class GetAllCountriesBody
    {
        public List<string>? Countries { get; set; }
    }
} 