using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRoutesByUserId
{
    public class GetRoutesByUserIdQueryHandler : IRequestHandler<GetRoutesByUserIdQueryRequest, GetRoutesByUserIdQueryResponse>
    {
        private readonly IRouteService _routeService;
        private readonly IHeaderService _headerService;


        public GetRoutesByUserIdQueryHandler(IRouteService routeService, IHeaderService headerService)
        {
            _routeService = routeService;
            _headerService = headerService;
        }

        public async Task<GetRoutesByUserIdQueryResponse> Handle(GetRoutesByUserIdQueryRequest request, CancellationToken cancellationToken)
        {
            var (routes, totalCount) = await _routeService.GetRoutesByUserId(
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