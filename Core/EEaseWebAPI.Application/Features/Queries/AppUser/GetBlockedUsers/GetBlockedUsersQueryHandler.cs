using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetBlockedUsers
{
    public class GetBlockedUsersQueryHandler : IRequestHandler<GetBlockedUsersQuery, GetBlockedUsersQueryResponse>
    {
        private readonly IFriendshipService _friendshipService;
        private readonly IHeaderService _headerService;

        public GetBlockedUsersQueryHandler(IFriendshipService friendshipService, IHeaderService headerService)
        {
            _friendshipService = friendshipService;
            _headerService = headerService;
        }

        public async Task<GetBlockedUsersQueryResponse> Handle(GetBlockedUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var blockedUsers = await _friendshipService.GetBlockedUsersAsync(request.Username);

                var blockedUserDtos = blockedUsers.Select(block => new BlockedUserDto
                {
                    Username = block.Blocked.UserName,
                    Name = block.Blocked.Name,
                    Surname = block.Blocked.Surname,
                    BlockedDate = block.BlockedDate
                }).ToList();

                return new GetBlockedUsersQueryResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.GetBlockedUsersSuccessfully),
                    Body = new GetBlockedUsersQueryResponseBody
                    {
                        BlockedUsers = blockedUserDtos
                    }
                };
            }
            catch (UserNotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get blocked users: {ex.Message}", ex);
            }
        }
    }
}
