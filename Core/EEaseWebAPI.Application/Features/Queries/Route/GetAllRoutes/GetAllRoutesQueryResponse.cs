using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetAllRoutes
{
    public class GetAllRoutesQueryResponse
    {
        public Header Header { get; set; }
        public GetAllRoutesQueryResponseBody Body { get; set; }
    }

    public class GetAllRoutesQueryResponseBody
    {
        public List<StandardRoute>? Routes { get; set; }
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalPages { get; set; }
        public bool HasPrevious { get; set; }
        public bool HasNext { get; set; }
    }
}
