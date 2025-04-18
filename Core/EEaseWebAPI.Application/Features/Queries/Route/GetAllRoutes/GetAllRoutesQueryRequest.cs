﻿using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetAllRoutes
{
    public class GetAllRoutesQueryRequest : IRequest<GetAllRoutesQueryResponse>
    {
        public string Username { get; set; }
        public int PageSize { get; set; }

        public int PageNumber { get; set; }
    }
}
