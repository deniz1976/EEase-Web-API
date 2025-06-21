using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetBlockedUsers
{
    public class GetBlockedUsersQuery : IRequest<GetBlockedUsersQueryResponse>
    {
        public string? Username { get; set; }
    }
} 