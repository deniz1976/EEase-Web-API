using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Route;
using MediatR;
using System;

namespace EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus
{
    public class UpdateRouteStatusCommandHandler : IRequestHandler<UpdateRouteStatusCommandRequest, UpdateRouteStatusCommandResponse>
    {
        private readonly IRouteInteractionService _routeInteractionService;
        private readonly IHeaderService _headerService;

        public UpdateRouteStatusCommandHandler(IRouteInteractionService routeInteractionService, IHeaderService headerService)
        {
            _routeInteractionService = routeInteractionService;
            _headerService = headerService;
        }

        public async Task<UpdateRouteStatusCommandResponse> Handle(UpdateRouteStatusCommandRequest request, CancellationToken cancellationToken)
        {
            if (request.Status < 0 || request.Status > 2)
            {
                throw new InvalidRouteStatusException();
            }

            var result = await _routeInteractionService.UpdateRouteStatusAsync(request.RouteId, request.Status, request.Username, cancellationToken);

            return new UpdateRouteStatusCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteStatusUpdatedSuccessfully),
                Body = new UpdateRouteStatusCommandResponseBody
                {
                    IsUpdated = result.IsUpdated
                }
            };
        }
    }
}
