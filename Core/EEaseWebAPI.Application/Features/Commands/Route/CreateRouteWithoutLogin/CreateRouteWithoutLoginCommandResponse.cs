using EEaseWebAPI.Application.DTOs.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin
{
    public class CreateRouteWithoutLoginCommandResponse
    {
        public Header? Header { get; set; }
        public CreateRouteWithoutLoginCommandResponseBody? Body { get; set; }
    }

    public class CreateRouteWithoutLoginCommandResponseBody 
    {
        public StandardRoute? Route { get; set; }
    }
}
