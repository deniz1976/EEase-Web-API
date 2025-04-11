using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant
{
    public class DislikePlaceOrRestaurantCommandRequest :IRequest<DislikePlaceOrRestaurantCommandResponse>
    {
        public string Username { get; set; }
        public string GooglePlaceId { get; set; }
        public string PlaceType { get; set; }

        public string? UserFeedback { get; set; }

        public int? DislikeType { get; set; }
    }
}
