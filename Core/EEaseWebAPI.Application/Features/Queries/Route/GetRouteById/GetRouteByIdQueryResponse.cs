using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRouteById
{
    public class GetRouteByIdQueryResponse  
    {
        public GetRouteByIdQueryResponseBody? Body { get; set; }
        public Header? Header { get; set; }

    }

    public class GetRouteByIdQueryResponseBody
    {
        public EEaseWebAPI.Application.DTOs.Route.StandardRouteDTO Route { get; set; }
    }
} 