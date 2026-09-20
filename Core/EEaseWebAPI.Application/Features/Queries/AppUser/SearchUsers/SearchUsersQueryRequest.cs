using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.SearchUsers
{
    /// <summary>
    /// A query, not an answer: it used to carry a Header and a Body of its own, which the
    /// controller filled in with an empty header nobody read.
    /// </summary>
    public class SearchUsersQueryRequest : IRequest<SearchUsersQueryResponse>
    {
        public string SearchTerm { get; set; } = string.Empty;
    }
}
