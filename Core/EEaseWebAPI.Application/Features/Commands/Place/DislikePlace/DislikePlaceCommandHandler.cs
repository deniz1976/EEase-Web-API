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
        private readonly IRouteDislikeService _routeDislikeService;

        public DislikePlaceCommandHandler(IHeaderService headerService, IRouteDislikeService routeDislikeService)
        {
            _headerService = headerService;
            _routeDislikeService = routeDislikeService;
        }

        public async Task<DislikePlaceCommandResponse> Handle(DislikePlaceCommandRequest request, CancellationToken cancellationToken)
        {
            return new DislikePlaceCommandResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.ComponentChangedSuccessfully),
                Body = new DislikePlaceCommandResponseBody()
                {
                    StandardRoute = await _routeDislikeService.DislikePlaceAsync(new()
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
