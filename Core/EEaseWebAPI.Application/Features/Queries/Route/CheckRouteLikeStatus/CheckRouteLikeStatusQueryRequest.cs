using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.Route.CheckRouteLikeStatus
{
    public class CheckRouteLikeStatusQueryRequest : IRequest<CheckRouteLikeStatusQueryResponse>
    {
        public string Username { get; set; }
        public Guid RouteId { get; set; }
    }
} 