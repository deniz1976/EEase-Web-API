using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.Route;
using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using MediatR;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute
{
    public class CreateCustomRouteCommandHandler : IRequestHandler<CreateCustomRouteCommandRequest, CreateCustomRouteCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly ICustomRouteService _customRouteService;

        public CreateCustomRouteCommandHandler(IHeaderService headerService , ICustomRouteService customRouteService)
        {
            _headerService = headerService;
            _customRouteService = customRouteService;
        }

        public async Task<CreateCustomRouteCommandResponse> Handle(CreateCustomRouteCommandRequest request, CancellationToken cancellationToken)
        {
            var route = await _customRouteService.CreatePrefRoute(
                request.destination,
                request.StartDate,
                request.EndDate,
                request.PRICE_LEVEL,
                request.username,
                request.usernames);

            return new CreateCustomRouteCommandResponse
            {
                Body = new() { Route = route },
                Header = _headerService.HeaderCreate((int)StatusEnum.RouteCreatedSuccessfully)
            };
        }
    }
}
