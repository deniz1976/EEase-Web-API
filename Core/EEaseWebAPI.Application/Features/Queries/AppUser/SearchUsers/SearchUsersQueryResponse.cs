using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.MapEntities;
using System.Collections.Generic;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.SearchUsers
{
    public class SearchUsersQueryResponse : ApiResponse<SearchUsersQueryResponseBody>
    {
    }

    public class SearchUsersQueryResponseBody
    {
        public List<UserSearchDTO> Users { get; set; } = new();
    }
}
