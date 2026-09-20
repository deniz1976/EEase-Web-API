using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRouteById
{
    public class GetRouteByIdQueryResponse : ApiResponse<GetRouteByIdQueryResponseBody>

    {

    }

    public class GetRouteByIdQueryResponseBody
    {
        public EEaseWebAPI.Application.DTOs.Route.StandardRouteDTO Route { get; set; }
    }
}
