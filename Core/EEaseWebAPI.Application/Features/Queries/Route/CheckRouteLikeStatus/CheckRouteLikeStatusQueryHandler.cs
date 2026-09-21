using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.CheckRouteLikeStatus
{
    public class CheckRouteLikeStatusQueryHandler : IRequestHandler<CheckRouteLikeStatusQueryRequest, CheckRouteLikeStatusQueryResponse>
    {
        private readonly IRouteQueryService _routeQueryService;
        private readonly IHeaderService _headerService;

        public CheckRouteLikeStatusQueryHandler(IRouteQueryService routeQueryService, IHeaderService headerService)
        {
            _routeQueryService = routeQueryService;
            _headerService = headerService;
        }

        public async Task<CheckRouteLikeStatusQueryResponse> Handle(CheckRouteLikeStatusQueryRequest request, CancellationToken cancellationToken)
        {
                var isLiked = await _routeQueryService.CheckRouteLikeStatus(request.Username, request.RouteId, cancellationToken);

                return new CheckRouteLikeStatusQueryResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.CheckRouteLikeSuccess),
                    Body = new CheckRouteLikeStatusQueryResponseBody
                    {
                        IsLiked = isLiked,
                        Message = isLiked ? AppMessages.RouteLiked : AppMessages.RouteNotLiked
                    }
                };
        }
    }
}
