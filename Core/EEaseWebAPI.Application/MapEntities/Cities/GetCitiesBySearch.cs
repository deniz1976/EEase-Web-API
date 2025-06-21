namespace EEaseWebAPI.Application.MapEntities.Cities
{
    public class GetCitiesBySearch
    {
        public Header Header { get; set; }
        public GetCitiesBySearchBody Body { get; set; }
    }

    public class GetCitiesBySearchBody
    {
        public List<CityDto> Cities { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class CityDto
    {
        public string CityName { get; set; }
        public string Country { get; set; }
    }
} 