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
        private readonly IRouteService _routeService;
        private readonly IHeaderService _headerService;


        public GetAllRoutesQueryHandler(IRouteService routeService, IHeaderService headerService)
        {
            _routeService = routeService;
            _headerService = headerService;
        }

        public async Task<GetAllRoutesQueryResponse> Handle(GetAllRoutesQueryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var (routes, totalCount) = await _routeService.GetAllRoutes(request.Username, request.PageNumber, request.PageSize, default);

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
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }
}
