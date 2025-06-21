using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute
{
    public class DeleteRouteCommandHandler : IRequestHandler<DeleteRouteCommandRequest, DeleteRouteCommandResponse>
    {
        private readonly IRouteService _routeService;
        private readonly IHeaderService _headerService;

        public DeleteRouteCommandHandler(IRouteService routeService, IHeaderService headerService)
        {
            _routeService = routeService;
            _headerService = headerService;
        }

        public async Task<DeleteRouteCommandResponse> Handle(DeleteRouteCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var isDeleted = await _routeService.DeleteRoute(request.Username, request.RouteId);

                return new DeleteRouteCommandResponse
                {
                    Body = new DeleteRouteCommandResponseBody
                    {
                        IsDeleted = isDeleted
                    },
                    Header = _headerService.HeaderCreate(isDeleted ? (int)StatusEnum.RouteDeletedSuccessfully : (int)StatusEnum.UnauthorizedToDeleteRoute)
                };
            }
            catch (Exception ex)
            {
                throw new Exception();
            }
        }
    }
} 