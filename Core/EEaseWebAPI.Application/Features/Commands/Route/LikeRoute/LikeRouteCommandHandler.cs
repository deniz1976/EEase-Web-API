using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikeRoute
{
    public class LikeRouteCommandHandler : IRequestHandler<LikeRouteCommandRequest, LikeRouteCommandResponse>
    {
        private readonly IRouteInteractionService _routeInteractionService;
        private readonly IHeaderService _headerService;

        public LikeRouteCommandHandler(IRouteInteractionService routeInteractionService, IHeaderService headerService)
        {
            _routeInteractionService = routeInteractionService;
            _headerService = headerService;
        }

        public async Task<LikeRouteCommandResponse> Handle(LikeRouteCommandRequest request, CancellationToken cancellationToken)
        {
            var isLiked = await _routeInteractionService.LikeRoute(request.Username, request.RouteId, cancellationToken);

            return new LikeRouteCommandResponse
            {
                Body = new LikeRouteCommandResponseBody
                {
                    IsLiked = isLiked
                },
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteLikeStatusUpdatedSuccessfully)
            };
        }
    }
}
