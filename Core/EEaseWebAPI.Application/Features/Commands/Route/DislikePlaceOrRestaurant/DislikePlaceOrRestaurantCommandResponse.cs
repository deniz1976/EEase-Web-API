using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Entities.Route;

namespace EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant
{
    public class DislikePlaceOrRestaurantCommandResponse : ApiResponse<DislikePlaceOrRestaurantCommandResponseBody>
    {
    }
    public class DislikePlaceOrRestaurantCommandResponseBody
    {
        public StandardRoute? StandardRoute { get; set; }
    }
}
