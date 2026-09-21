using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Route;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRouteById
{
    public class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQueryRequest, GetRouteByIdQueryResponse>
    {
        private readonly IRouteQueryService _routeQueryService;
        private readonly IHeaderService _headerService;

        public GetRouteByIdQueryHandler(IRouteQueryService routeQueryService, IHeaderService headerService)
        {
            _routeQueryService = routeQueryService;
            _headerService = headerService;
        }

        public async Task<GetRouteByIdQueryResponse> Handle(GetRouteByIdQueryRequest request, CancellationToken cancellationToken)
        {
            var route = await _routeQueryService.GetRouteById(request.Username, request.RouteId, cancellationToken);

            return new GetRouteByIdQueryResponse
            {
                Body = new GetRouteByIdQueryResponseBody
                {
                    Route = route
                },
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteRetrievedSuccessfully)
            };
        }
    }
}
