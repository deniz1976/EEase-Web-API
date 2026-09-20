using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.Route.CheckRouteLikeStatus
{
    public class CheckRouteLikeStatusQueryResponse : ApiResponse<CheckRouteLikeStatusQueryResponseBody>
    {
    }

    public class CheckRouteLikeStatusQueryResponseBody
    {
        public bool IsLiked { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}
