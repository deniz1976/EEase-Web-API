using EEaseWebAPI.Application.MapEntities;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetBlockedUsers
{
    public class GetBlockedUsersQueryResponse
    {
        public Header? Header { get; set; }
        public GetBlockedUsersQueryResponseBody? Body { get; set; }
    }

    public class GetBlockedUsersQueryResponseBody
    {
        public List<BlockedUserDto> BlockedUsers { get; set; } = [];
    }

    public class BlockedUserDto
    {
        public string? Username { get; set; }
        public string? Name { get; set; }
        public string? Surname { get; set; }
        public DateTime? BlockedDate { get; set; }
    }
} 