using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Entities.Route;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin
{
    public class CreateRouteWithoutLoginCommandResponse : ApiResponse<CreateRouteWithoutLoginCommandResponseBody>
    {
    }

    public class CreateRouteWithoutLoginCommandResponseBody
    {
        public StandardRoute? Route { get; set; }
    }
}
