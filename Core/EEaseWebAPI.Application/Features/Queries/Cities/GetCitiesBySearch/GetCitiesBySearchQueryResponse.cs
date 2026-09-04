using System.Collections.Generic;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.Cities;

namespace EEaseWebAPI.Application.Features.Queries.Cities.GetCitiesBySearch
{
    public class GetCitiesBySearchQueryResponse
    {
        public Header? Header { get; set; }
        public GetCitiesBySearchQueryResponseBody? Body { get; set; }
    }

    public class GetCitiesBySearchQueryResponseBody
    {
        public List<CityDto> Cities { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }
} 