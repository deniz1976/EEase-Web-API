using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Route;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.GetRouteById
{
    public class GetRouteByIdQueryHandler : IRequestHandler<GetRouteByIdQueryRequest, GetRouteByIdQueryResponse>
    {
        private readonly IRouteService _routeService;
        private readonly IHeaderService _headerService;

        public GetRouteByIdQueryHandler(IRouteService routeService, IHeaderService headerService)
        {
            _routeService = routeService;
            _headerService = headerService;
        }

        public async Task<GetRouteByIdQueryResponse> Handle(GetRouteByIdQueryRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var route = await _routeService.GetRouteById(request.Username, request.RouteId);

                return new GetRouteByIdQueryResponse
                {
                    Body = new GetRouteByIdQueryResponseBody
                    {
                        Route = route
                    },
                    Header = _headerService.HeaderCreate((int)StatusEnum.RouteRetrievedSuccessfully)
                };
            }
            catch (UnauthorizedAccessException)
            {
                throw new GetRouteException();
            }
            catch (Exception)
            {
                throw new Exception();
            }
        }
    }
} 