using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.SearchUsers
{
    public class SearchUsersQueryRequest : IRequest<SearchUsersQueryResponse>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }
}
