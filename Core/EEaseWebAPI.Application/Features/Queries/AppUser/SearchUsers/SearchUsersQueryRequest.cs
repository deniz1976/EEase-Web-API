using EEaseWebAPI.Application.MapEntities;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.SearchUsers
{
    public class SearchUsersQueryRequest : IRequest<SearchUsersQueryResponse>
    {
        public Header Header { get; set; }
        public SearchUsersQueryRequestBody Body { get; set; }
    }

    public class SearchUsersQueryRequestBody
    {
        public string SearchTerm { get; set; }
    }
} 