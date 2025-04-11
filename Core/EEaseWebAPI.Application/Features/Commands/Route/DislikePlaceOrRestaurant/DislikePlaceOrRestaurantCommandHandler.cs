using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant
{
    public class DislikePlaceOrRestaurantCommandHandler : IRequestHandler<DislikePlaceOrRestaurantCommandRequest, DislikePlaceOrRestaurantCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IRouteService _routeService;

        public DislikePlaceOrRestaurantCommandHandler(IHeaderService headerService, IRouteService routeService)
        {
            _headerService = headerService;
            _routeService = routeService;
        }

        public async Task<DislikePlaceOrRestaurantCommandResponse> Handle(DislikePlaceOrRestaurantCommandRequest request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
