using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteAllRoutes
{
    public class DeleteAllRoutesCommandHandler : IRequestHandler<DeleteAllRoutesCommandRequest, DeleteAllRoutesCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IRouteService _routeService;

        public DeleteAllRoutesCommandHandler(IHeaderService headerService, IRouteService routeService)
        {
            _headerService = headerService;
            _routeService = routeService;
        }
        public async Task<DeleteAllRoutesCommandResponse> Handle(DeleteAllRoutesCommandRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.Username == null) 
            {
                throw new ArgumentNullException(nameof(request));
            }

            var response = await _routeService.DeleteAllRoutes(request.Username);

            return new DeleteAllRoutesCommandResponse
            {
                Header = _headerService.HeaderCreate(),
                Body = new DeleteAllRoutesCommandResponseBody
                {
                    Message = response
                }
            };
        }
    }
}
