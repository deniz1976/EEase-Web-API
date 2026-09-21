using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetAllRoutes
{
    public class GetAllRoutesQueryHandler : IRequestHandler<GetAllRoutesQueryRequest, GetAllRoutesQueryResponse>
    {
        private readonly IRouteQueryService _routeQueryService;
        private readonly IHeaderService _headerService;

        public GetAllRoutesQueryHandler(IRouteQueryService routeQueryService, IHeaderService headerService)
        {
            _routeQueryService = routeQueryService;
            _headerService = headerService;
        }

        public async Task<GetAllRoutesQueryResponse> Handle(GetAllRoutesQueryRequest request, CancellationToken cancellationToken)
        {
            var (routes, totalCount) = await _routeQueryService.GetAllRoutes(
                request.Username, request.PageNumber, request.PageSize, cancellationToken);

            var totalPages = (int)Math.Ceiling(totalCount / (double)request.PageSize);

            return new GetAllRoutesQueryResponse
            {
                Body = new GetAllRoutesQueryResponseBody
                {
                    Routes = routes,
                    TotalCount = totalCount,
                    CurrentPage = request.PageNumber,
                    PageSize = request.PageSize,
                    TotalPages = totalPages,
                    HasPrevious = request.PageNumber > 1,
                    HasNext = request.PageNumber < totalPages
                },
                Header = _headerService.HeaderCreate((int)StatusEnum.GetAllRoutesRetrievedSuccessfully)
            };
        }
    }
}
