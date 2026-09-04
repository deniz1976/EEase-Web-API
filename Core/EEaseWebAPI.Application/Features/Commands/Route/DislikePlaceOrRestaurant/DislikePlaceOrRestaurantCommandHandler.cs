using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant
{
    public class DislikePlaceOrRestaurantCommandHandler : IRequestHandler<DislikePlaceOrRestaurantCommandRequest, DislikePlaceOrRestaurantCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IRouteService _routeService;

        public DislikePlaceOrRestaurantCommandHandler(IHeaderService headerService, IRouteService routeService)
        {
            _headerService = headerService;
            _routeService = routeService;
        }

        public async Task<DislikePlaceOrRestaurantCommandResponse> Handle(DislikePlaceOrRestaurantCommandRequest request, CancellationToken cancellationToken)
        {
            return new DislikePlaceOrRestaurantCommandResponse() 
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.ComponentChangedSuccessfully),
                Body = new DislikePlaceOrRestaurantCommandResponseBody()
                {
                    StandardRoute = await _routeService.DislikePlaceOrRestaurant(new() 
                    {
                        Username = request.Username,
                        RouteId = request.RouteId,
                        GooglePlaceId = request.GooglePlaceId,
                        PlaceType = request.PlaceType,
                        UserFeedback = request.UserFeedback,
                        DislikeType = request.DislikeType
                    })
                }
            };
        }
    }
}
