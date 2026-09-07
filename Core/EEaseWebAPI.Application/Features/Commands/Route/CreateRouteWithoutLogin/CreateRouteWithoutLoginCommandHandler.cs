using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Route;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin
{
    public class CreateRouteWithoutLoginCommandHandler : IRequestHandler<CreateRouteWithoutLoginCommandRequest, CreateRouteWithoutLoginCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly ICustomRouteService _customRouteService;

        public CreateRouteWithoutLoginCommandHandler(IHeaderService headerService, ICustomRouteService customRouteService)
        {
            _headerService = headerService;
            _customRouteService = customRouteService;
        }

        public async Task<CreateRouteWithoutLoginCommandResponse> Handle(CreateRouteWithoutLoginCommandRequest request, CancellationToken cancellationToken)
        {
            var route = await _customRouteService.CreateRandomRoute(
                request.destination!,
                request.StartDate,
                request.EndDate,
                request.PRICE_LEVEL);

            return new CreateRouteWithoutLoginCommandResponse
            {
                Body = new() { Route = route },
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteCreatedSuccessfully)
            };
        }
    }
}
