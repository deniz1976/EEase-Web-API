using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Queries.Route.CheckRouteLikeStatus
{
    public class CheckRouteLikeStatusQueryHandler : IRequestHandler<CheckRouteLikeStatusQueryRequest, CheckRouteLikeStatusQueryResponse>
    {
        private readonly IRouteService _routeService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public CheckRouteLikeStatusQueryHandler(IRouteService routeService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _routeService = routeService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<CheckRouteLikeStatusQueryResponse> Handle(CheckRouteLikeStatusQueryRequest request, CancellationToken cancellationToken)
        {

                var isLiked = await _routeService.CheckRouteLikeStatus(request.Username, request.RouteId);

                return new CheckRouteLikeStatusQueryResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.CheckRouteLikeSuccess),
                    Body = new CheckRouteLikeStatusQueryResponseBody
                    {
                        IsLiked = isLiked,
                        Message = isLiked ? _messages["RouteLiked"] : _messages["RouteNotLiked"]
                    }
                };

        }
    }
}
