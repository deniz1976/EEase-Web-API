using System.Collections.Generic;
using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AllWorldCities.GetAllCountries
{
    public class GetAllCountriesQueryResponse : ApiResponse<GetAllCountriesQueryResponseBody>
    {
    }

    public class GetAllCountriesQueryResponseBody
    {
        public List<string>? Countries { get; set; }
    }
}
