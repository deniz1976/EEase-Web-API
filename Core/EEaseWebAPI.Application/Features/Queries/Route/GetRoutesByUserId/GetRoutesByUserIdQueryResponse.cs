using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRoutesByUserId
{
    public class GetRoutesByUserIdQueryResponse
    {
        public Header Header { get; set; }
        public GetRoutesByUserIdQueryResponseBody Body { get; set; }
    }

    public class GetRoutesByUserIdQueryResponseBody
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