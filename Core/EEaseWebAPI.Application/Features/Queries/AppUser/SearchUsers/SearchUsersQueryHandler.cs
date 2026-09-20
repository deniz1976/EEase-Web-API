using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.SearchUsers
{
    public class SearchUsersQueryHandler : IRequestHandler<SearchUsersQueryRequest, SearchUsersQueryResponse>
    {
        private readonly IUserCacheService _userCacheService;

        public SearchUsersQueryHandler(IUserCacheService userCacheService)
        {
            _userCacheService = userCacheService;
        }

        public async Task<SearchUsersQueryResponse> Handle(SearchUsersQueryRequest request, CancellationToken cancellationToken)
        {
            var users = await _userCacheService.SearchUsersAsync(request.Body.SearchTerm);

            return new SearchUsersQueryResponse
            {
                Header = new Header
                {
                    Success = true,
                    ResponseDate = DateTime.UtcNow,
                    EnumStatusCode = (int)StatusEnum.SearchUsersSuccessfully
                },
                Body = new SearchUsersQueryResponseBody
                {
                    Users = users
                }
            };
        }
    }
}
