using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant
{
    // 0 : dont want to answer
    // 1 : Place closed permanently
    // 2 : Place is not in destination city
    // 3 : Place irrelevant
    // 4 : place recommended very often
    // 5 : too far
    // 6 : not available at the moment 
    // 7 : has bad reviews 
    // 8 : dont like it
    // 9 : other
    public class DislikePlaceOrRestaurantCommandRequest :IRequest<DislikePlaceOrRestaurantCommandResponse>
    {
        public string? Username { get; set; }
        public string? RouteId { get; set; }
        public string? GooglePlaceId { get; set; }
        public string? PlaceType { get; set; }
        public string? UserFeedback { get; set; }
        public int? DislikeType { get; set; }
    }
}
