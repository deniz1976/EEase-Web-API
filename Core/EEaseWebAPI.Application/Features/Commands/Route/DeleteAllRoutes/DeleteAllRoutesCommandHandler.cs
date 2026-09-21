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
        private readonly IRouteInteractionService _routeInteractionService;

        public DeleteAllRoutesCommandHandler(IHeaderService headerService, IRouteInteractionService routeInteractionService)
        {
            _headerService = headerService;
            _routeInteractionService = routeInteractionService;
        }
        public async Task<DeleteAllRoutesCommandResponse> Handle(DeleteAllRoutesCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _routeInteractionService.DeleteAllRoutes(request.Username, cancellationToken);

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
