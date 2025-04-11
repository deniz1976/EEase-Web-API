using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.Route.CheckRouteLikeStatus
{
    public class CheckRouteLikeStatusQueryResponse
    {
        public Header Header { get; set; }
        public CheckRouteLikeStatusQueryResponseBody Body { get; set; }
    }

    public class CheckRouteLikeStatusQueryResponseBody
    {
        public bool IsLiked { get; set; }
        public string Message { get; set; }
    }
}
