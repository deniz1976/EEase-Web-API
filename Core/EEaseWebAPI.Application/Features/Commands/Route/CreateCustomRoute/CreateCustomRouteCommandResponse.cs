using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute
{
    public class CreateCustomRouteCommandResponse
    {
        public Header? Header { get; set; }

        public CreateCustomRouteCommandResponseBody? Body { get; set; }
    }

    public class CreateCustomRouteCommandResponseBody 
    {
        public StandardRoute? Route { get; set; }
    }
}
