using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetPendingFriendRequests
{
    public class GetPendingFriendRequestsQueryHandler : IRequestHandler<GetPendingFriendRequestsQuery, GetPendingFriendRequestsQueryResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public GetPendingFriendRequestsQueryHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<GetPendingFriendRequestsQueryResponse> Handle(GetPendingFriendRequestsQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pendingRequests = await _userService.GetPendingFriendRequestsAsync(request.Username);
                var pendingRequestDtos = pendingRequests.Select(fr => new PendingFriendRequestDto
                {
                    RequesterUsername = fr.Requester.UserName,
                    RequesterName = fr.Requester.Name,
                    RequesterSurname = fr.Requester.Surname,
                    RequestDate = fr.RequestDate
                }).ToList();

                return new GetPendingFriendRequestsQueryResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.GetPendingRequestSuccessfully),
                    Body = new GetPendingFriendRequestsQueryResponseBody
                    {
                        PendingRequests = pendingRequestDtos
                    }
                };
            }
            catch (UserNotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get pending friend requests: {ex.Message}", ex);
            }
        }
    }
} 