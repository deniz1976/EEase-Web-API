using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute
{
    public class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommandRequest, DeleteRouteCommandResponse>
    {
        private readonly IRouteInteractionService _routeInteractionService;
        private readonly IHeaderService _headerService;

        public DeleteRouteCommandHandler(IRouteInteractionService routeInteractionService, IHeaderService headerService)
        {
            _routeInteractionService = routeInteractionService;
            _headerService = headerService;
        }

        public async Task<DeleteRouteCommandResponse> Handle(DeleteRouteCommandRequest request, CancellationToken cancellationToken)
        {
            var isDeleted = await _routeInteractionService.DeleteRoute(request.Username, request.RouteId, cancellationToken);

            return new DeleteRouteCommandResponse
            {
                Body = new DeleteRouteCommandResponseBody
                {
                    IsDeleted = isDeleted
                },
                Header = _headerService.HeaderCreate(isDeleted ? (int)StatusEnum.RouteDeletedSuccessfully : (int)StatusEnum.UnauthorizedToDeleteRoute)
            };
        }
    }
}
