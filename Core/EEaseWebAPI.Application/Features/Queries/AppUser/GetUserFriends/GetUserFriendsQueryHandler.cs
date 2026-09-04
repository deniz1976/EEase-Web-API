using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserFriends
{
    public class GetUserFriendsQueryHandler : IRequestHandler<GetUserFriendsQuery, GetUserFriendsQueryResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;
        private readonly UserManager<Domain.Entities.Identity.AppUser> _userManager;

        public GetUserFriendsQueryHandler(IUserService userService, IHeaderService headerService, UserManager<Domain.Entities.Identity.AppUser> userManager)
        {
            _userService = userService;
            _headerService = headerService;
            _userManager = userManager;
        }

        public async Task<GetUserFriendsQueryResponse> Handle(GetUserFriendsQuery request, CancellationToken cancellationToken)
        {

            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                throw new UserNotFoundException();
            try
            {
                var friends = await _userService.GetUserFriendsAsync(request.Username);
                var friendDtos = friends.Select(f =>
                {
                    var friend = f.RequesterId == user.Id ? f.Addressee : f.Requester;
                    return new UserFriendDto
                    {
                        Username = friend.UserName,
                        Name = friend.Name,
                        Surname = friend.Surname,
                        PhotoPath = friend.PhotoPath,
                        FriendshipDate = f.ResponseDate ?? f.RequestDate
                    };
                }).ToList();

                

                return new GetUserFriendsQueryResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.GetUserFriendsSuccessfully),
                    Body = new GetUserFriendsQueryResponseBody
                    {
                        Friends = friendDtos
                    }
                };
            }
            catch (UserNotFoundException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to get user friends: {ex.Message}", ex);
            }
        }
    }
} 