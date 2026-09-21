using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRoutesByUserId
{
    public class GetRoutesByUserIdQueryHandler : IRequestHandler<GetRoutesByUserIdQueryRequest, GetRoutesByUserIdQueryResponse>
    {
        private readonly IRouteQueryService _routeQueryService;
        private readonly IHeaderService _headerService;

        public GetRoutesByUserIdQueryHandler(IRouteQueryService routeQueryService, IHeaderService headerService)
        {
            _routeQueryService = routeQueryService;
            _headerService = headerService;
        }

        public async Task<GetRoutesByUserIdQueryResponse> Handle(GetRoutesByUserIdQueryRequest request, CancellationToken cancellationToken)
        {
            var (routes, totalCount) = await _routeQueryService.GetRoutesByUserId(
                request.UserId,
                request.RequesterUsername,
                request.PageNumber,
                request.PageSize,
                cancellationToken);

            return new GetRoutesByUserIdQueryResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.GetRoutesByUserIdRetrievedSuccessfully),
                Body = new GetRoutesByUserIdQueryResponseBody
                {
                    Routes = routes,
                    TotalCount = totalCount,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize),
                    HasPrevious = request.PageNumber > 1,
                    HasNext = request.PageNumber < (int)Math.Ceiling(totalCount / (double)request.PageSize)
                }
            };
        }
    }
}
