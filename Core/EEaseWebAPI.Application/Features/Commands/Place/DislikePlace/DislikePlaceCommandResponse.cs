using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Features.Commands.Place.DislikePlace
{
    public class DislikePlaceCommandResponse : ApiResponse<DislikePlaceCommandResponseBody>
    {
    }
    public class DislikePlaceCommandResponseBody
    {
        public StandardRoute? StandardRoute { get; set; }
    }
}
