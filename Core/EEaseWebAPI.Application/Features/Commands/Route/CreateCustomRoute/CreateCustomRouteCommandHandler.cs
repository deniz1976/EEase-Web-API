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
        //private readonly IRouteService _routeService;
        private readonly ICustomRouteService _customRouteService;

        //public CreateCustomRouteCommandHandler(IHeaderService headerService, IRouteService routeService, ICustomRouteService customRouteService)
        //{
        //    _headerService = headerService;
        //    _routeService = routeService;
        //    _customRouteService = customRouteService;
        //}

        public CreateCustomRouteCommandHandler(IHeaderService headerService , ICustomRouteService customRouteService)
        {
            _headerService = headerService;
            _customRouteService = customRouteService;
        }

        public async Task<CreateCustomRouteCommandResponse> Handle(CreateCustomRouteCommandRequest request, CancellationToken cancellationToken)
        {
            var result = RequestValidate(request);
            if (result)
            {
                return new CreateCustomRouteCommandResponse()
                {
                    Body = new() 
                    {
                        Route = await _customRouteService.CreatePrefRoute(request.destination, request.StartDate, request.EndDate, request.PRICE_LEVEL, request.username, request.usernames)
                    },
                    Header = _headerService.HeaderCreate((int)StatusEnum.RouteCreatedSuccessfully),
                };
            }

            throw new CreateRouteRequestException();
        }

        private bool RequestValidate(CreateCustomRouteCommandRequest request) 
        {
            if (request == null || String.IsNullOrEmpty(request.destination) || request.StartDate == null || request.EndDate == null) throw new ArgumentNullException();

            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var oneYearLater = today.AddYears(1);

            if (request.StartDate < today || request.EndDate < today)
                throw new CreateRouteRequestException("You cant create route for past.");

            if (request.StartDate > oneYearLater || request.EndDate > oneYearLater)
                throw new CreateRouteRequestException("Routes cannot be created for dates more than 1 year away.");

            if (request.StartDate > request.EndDate)
                throw new CreateRouteRequestException("The start date cannot be later than the end date.");

            var startDateTime = request.StartDate.Value.ToDateTime(TimeOnly.MinValue);
            var endDateTime = request.EndDate.Value.ToDateTime(TimeOnly.MinValue);
            var dayDifference = (endDateTime - startDateTime).Days;
            if (dayDifference >= 5)
                throw new CreateRouteRequestException("The route can be a maximum of 5 days.");

            if (request.usernames.Count >= 5 || request.usernames.Count < 0)
                throw new CreateRouteRequestException("Route person count cant be >= 5 or < 0");

            return true;

        }
    }
}
