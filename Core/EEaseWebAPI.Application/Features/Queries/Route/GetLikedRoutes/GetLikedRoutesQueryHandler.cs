using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Features.Queries.Route.GetAllRoutes;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetLikedRoutes
{
    public class GetLikedRoutesQueryHandler : IRequestHandler<GetLikedRoutesQueryRequest, GetLikedRoutesQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IRouteQueryService _routeQueryService;

        public GetLikedRoutesQueryHandler(IHeaderService headerService, IRouteQueryService routeQueryService)
        {
            _headerService = headerService;
            _routeQueryService = routeQueryService;
        }

        public async Task<GetLikedRoutesQueryResponse> Handle(GetLikedRoutesQueryRequest request, CancellationToken cancellationToken)
        {
            var (routes, totalCount) = await _routeQueryService.GetLikedRoutes(
                request.Username, request.PageNumber, request.PageSize, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new GetLikedRoutesQueryResponse
            {
                Body = new GetLikedRoutesQueryResponseBody
                {
                    Routes = routes,
                    TotalCount = totalCount,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPrevious = request.PageNumber > 1,
                    HasNext = request.PageNumber < totalPages
                },
                Header = _headerService.HeaderCreate((int)StatusEnum.GetLikedRoutesRetrievedSuccessfully)
            };
        }
    }
}
