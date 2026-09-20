namespace EEaseWebAPI.Application.MapEntities.Cities
{

    public class SearchCitiesBody
    {
        public List<CityDto> Cities { get; set; } = new();
        public int TotalCount { get; set; }
        public int PageSize { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
    }

    public class CityDto
    {
        public string CityName { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
