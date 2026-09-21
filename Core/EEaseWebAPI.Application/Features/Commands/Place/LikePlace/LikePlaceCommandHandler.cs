using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Place.LikePlace
{
    public class LikePlaceCommandHandler : IRequestHandler<LikePlaceCommandRequest, LikePlaceCommandResponse>
    {
        private readonly IRouteInteractionService _routeInteractionService;
        private readonly IHeaderService _headerService;

        public LikePlaceCommandHandler(IRouteInteractionService routeInteractionService, IHeaderService headerService)
        {
            _routeInteractionService = routeInteractionService;
            _headerService = headerService;
        }

        public async Task<LikePlaceCommandResponse> Handle(LikePlaceCommandRequest request, CancellationToken cancellationToken)
        {
                var result = await _routeInteractionService.LikePlaceAsync(
                    request.Username,
                    request.GooglePlaceId,
                    request.PlaceType,
                    cancellationToken);

                return new LikePlaceCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PreferenceUpdatedSuccessfully),
                    Body = result
                };

        }
    }
}
