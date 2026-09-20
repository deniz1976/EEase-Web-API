using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Place.DislikePlace
{
    public class DislikePlaceCommandHandler : IRequestHandler<DislikePlaceCommandRequest, DislikePlaceCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IRouteService _routeService;

        public DislikePlaceCommandHandler(IHeaderService headerService, IRouteService routeService)
        {
            _headerService = headerService;
            _routeService = routeService;
        }

        public async Task<DislikePlaceCommandResponse> Handle(DislikePlaceCommandRequest request, CancellationToken cancellationToken)
        {
            return new DislikePlaceCommandResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.ComponentChangedSuccessfully),
                Body = new DislikePlaceCommandResponseBody()
                {
                    StandardRoute = await _routeService.DislikePlaceAsync(new()
                    {
                        Username = request.Username,
                        RouteId = request.RouteId,
                        GooglePlaceId = request.GooglePlaceId,
                        PlaceType = request.PlaceType,
                        UserFeedback = request.UserFeedback,
                        DislikeType = request.DislikeType
                    }, cancellationToken)
                }
            };
        }
    }
}
