using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Cities.GetCitiesBySearch
{
    public class GetCitiesBySearchQueryRequest : IRequest<GetCitiesBySearchQueryResponse>
    {
        public string SearchTerm { get; set; }
        public int PageSize { get; set; } = 10;
        public int PageNumber { get; set; } = 1;
        public string? Username { get; set; }
    }
} 