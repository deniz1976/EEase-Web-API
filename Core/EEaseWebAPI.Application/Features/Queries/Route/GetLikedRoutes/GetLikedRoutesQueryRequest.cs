using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetLikedRoutes
{
    public class GetLikedRoutesQueryRequest : IRequest<GetLikedRoutesQueryResponse>
    {
        public string Username { get; set; } = string.Empty;
        public int PageSize { get; set; }
        public int PageNumber { get; set;}
    }
}
