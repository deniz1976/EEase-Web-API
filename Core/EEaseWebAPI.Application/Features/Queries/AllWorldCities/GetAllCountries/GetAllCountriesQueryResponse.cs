using System.Collections.Generic;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AllWorldCities.GetAllCountries
{
    public class GetAllCountriesQueryResponse
    {
        public Header? Header { get; set; }
        public GetAllCountriesQueryResponseBody? Body { get; set; }
    }

    public class GetAllCountriesQueryResponseBody
    {
        public List<string>? Countries { get; set; }
    }
} 