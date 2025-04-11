using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.LikeRoute
{
    public class LikeRouteCommandHandler : IRequestHandler<LikeRouteCommandRequest, LikeRouteCommandResponse>
    {
        private readonly IRouteService _routeService;
        private readonly IHeaderService _headerService;

        public LikeRouteCommandHandler(IRouteService routeService, IHeaderService headerService)
        {
            _routeService = routeService;
            _headerService = headerService;
        }

        public async Task<LikeRouteCommandResponse> Handle(LikeRouteCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var isLiked = await _routeService.LikeRoute(request.Username, request.RouteId);

                return new LikeRouteCommandResponse
                {
                    Body = new LikeRouteCommandResponseBody
                    {
                        IsLiked = isLiked
                    },
                    Header = _headerService.HeaderCreate((int)StatusEnum.RouteLikeStatusUpdatedSuccessfully)
                };
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }
} 