using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.MapEntities;
using System.Collections.Generic;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.SearchUsers
{
    public class SearchUsersQueryResponse
    {
        public Header Header { get; set; }
        public SearchUsersQueryResponseBody Body { get; set; }
    }

    public class SearchUsersQueryResponseBody
    {
        public List<UserSearchDTO> Users { get; set; }
    }
} 