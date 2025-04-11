using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetBlockedUsers
{
    public class GetBlockedUsersQueryHandler : IRequestHandler<GetBlockedUsersQuery, GetBlockedUsersQueryResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public GetBlockedUsersQueryHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<GetBlockedUsersQueryResponse> Handle(GetBlockedUsersQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var blockedUsers = await _userService.GetBlockedUsersAsync(request.Username);

                var blockedUserDtos = blockedUsers.Select(f => new BlockedUserDto
                {
                    Username = f.Addressee.UserName,
                    Name = f.Addressee.Name,
                    Surname = f.Addressee.Surname,
                    BlockedDate = f.ResponseDate ?? f.RequestDate
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