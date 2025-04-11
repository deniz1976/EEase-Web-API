using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.MapEntities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetLikedRoutes
{
    public class GetLikedRoutesQueryResponse
    {
        public Header? Header { get; set; }
        public GetLikedRoutesQueryResponseBody? Body { get; set; }
    }

    public class GetLikedRoutesQueryResponseBody 
    {
        public List<StandardRouteDTO> Routes { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
