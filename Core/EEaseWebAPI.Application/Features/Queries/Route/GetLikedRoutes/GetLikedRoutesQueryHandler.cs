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
        private readonly IRouteService _routeService;

        public GetLikedRoutesQueryHandler(IHeaderService headerService, IRouteService routeService)
        {
            _headerService = headerService;
            _routeService = routeService;
        }

        public async Task<GetLikedRoutesQueryResponse> Handle(GetLikedRoutesQueryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (routes, totalCount) = await _routeService.GetLikedRoutes(request.Username, request.PageNumber, request.PageSize, default);

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
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }
}
