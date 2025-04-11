using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant
{
    public class LikePlaceOrRestaurantCommandHandler : IRequestHandler<LikePlaceOrRestaurantCommandRequest, LikePlaceOrRestaurantCommandResponse>
    {
        private readonly IRouteService _routeService;
        private readonly IHeaderService _headerService;

        public LikePlaceOrRestaurantCommandHandler(IRouteService routeService, IHeaderService headerService)
        {
            _routeService = routeService;
            _headerService = headerService;
        }

        public async Task<LikePlaceOrRestaurantCommandResponse> Handle(LikePlaceOrRestaurantCommandRequest request, CancellationToken cancellationToken)
        {
            
                var result = await _routeService.LikePlaceOrRestaurantAsync(
                    request.Username,
                    request.GooglePlaceId,
                    request.PlaceType
                );

                return new LikePlaceOrRestaurantCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PreferenceUpdatedSuccessfully),
                    Body = result
                };
            
        }
    }
} 