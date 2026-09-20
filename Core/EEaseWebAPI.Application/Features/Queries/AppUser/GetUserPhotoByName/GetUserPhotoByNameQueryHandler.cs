using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhotoByName
{
    public class GetUserPhotoByNameQueryHandler : IRequestHandler<GetUserPhotoByNameQueryRequest, GetUserPhotoByNameQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;
        private readonly IFriendshipService _friendshipService;

        public GetUserPhotoByNameQueryHandler(
            IHeaderService headerService,
            IUserProfileService profileService,
            IFriendshipService friendshipService)
        {
            _headerService = headerService;
            _profileService = profileService;
            _friendshipService = friendshipService;
        }

        public async Task<GetUserPhotoByNameQueryResponse> Handle(GetUserPhotoByNameQueryRequest request, CancellationToken cancellationToken)
        {
var isSelf = request.username == request.targetUsername;

            if (!isSelf && !await _friendshipService.AreFriendsAsync(request.username, request.targetUsername))
            {
                return new GetUserPhotoByNameQueryResponse()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.NotFriends),
                    Body = new()
                    {
                        path = null,
                        errorMessage = AppMessages.NotFriendsPhoto
                    }
                };
            }

            return new GetUserPhotoByNameQueryResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserPhotoReceivedSuccessfully),
                Body = new() { path = await _profileService.GetUserPhotoAsync(request.targetUsername) }
            };
        }
    }
}
