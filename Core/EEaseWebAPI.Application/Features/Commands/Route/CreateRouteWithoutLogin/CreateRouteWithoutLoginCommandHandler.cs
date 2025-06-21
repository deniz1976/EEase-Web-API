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
            if(RequestControl(request)) 
            {
                return new CreateRouteWithoutLoginCommandResponse()
                {
                    Body = new()
                    {
                        Route = await _customRouteService.CreateRandomRoute(request.destination, request.StartDate, request.EndDate, request.PRICE_LEVEL)
                    },
                    Header = _headerService.HeaderCreate((int)StatusEnum.RouteCreatedSuccessfully)
                };
            }

            throw new CreateRouteRequestException();
        }

        public static bool RequestControl(CreateRouteWithoutLoginCommandRequest request)
        {
            if (request == null || String.IsNullOrEmpty(request.destination) || request.StartDate == null || request.EndDate == null) throw new ArgumentNullException();

            //if (!(request.PRICE_LEVEL == Domain.Entities.Route.PRICE_LEVEL.PRICE_LEVEL_INEXPENSIVE || 
            //        request.PRICE_LEVEL == Domain.Entities.Route.PRICE_LEVEL.PRICE_LEVEL_EXPENSIVE   ||
            //        request.PRICE_LEVEL == Domain.Entities.Route.PRICE_LEVEL.PRICE_LEVEL_MODERATE)) 
            //{
            //    throw new CreateRouteRequestException("Please use valid price level enum.");
            //}

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
            if (dayDifference > 5)
                throw new CreateRouteRequestException("The route can be a maximum of 5 days.");

            return true;
        }
    }
}
