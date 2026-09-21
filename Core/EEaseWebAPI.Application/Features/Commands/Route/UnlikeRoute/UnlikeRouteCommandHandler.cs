using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.UnlikeRoute
{
    public class UnlikeRouteCommandHandler : IRequestHandler<UnlikeRouteCommandRequest, UnlikeRouteCommandResponse>
    {
        private readonly IRouteInteractionService _routeInteractionService;
        private readonly IHeaderService _headerService;

        public UnlikeRouteCommandHandler(
            IRouteInteractionService routeInteractionService, IHeaderService headerService)
        {
            _routeInteractionService = routeInteractionService;
            _headerService = headerService;
        }

        public async Task<UnlikeRouteCommandResponse> Handle(
            UnlikeRouteCommandRequest request, CancellationToken cancellationToken)
        {
            var likeCount = await _routeInteractionService.UnlikeRouteAsync(
                request.Username, request.RouteId, cancellationToken);

            return new UnlikeRouteCommandResponse
            {
                Body = new RouteLikeBody { IsLiked = false, LikeCount = likeCount },
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteLikeStatusUpdatedSuccessfully)
            };
        }
    }
}
