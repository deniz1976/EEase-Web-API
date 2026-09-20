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
            var isSelf = request.Username == request.TargetUsername;

            if (!isSelf && !await _friendshipService.AreFriendsAsync(request.Username, request.TargetUsername, cancellationToken))
            {
                return new GetUserPhotoByNameQueryResponse()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.NotFriends),
                    Body = new()
                    {
                        Path = null,
                        ErrorMessage = AppMessages.NotFriendsPhoto
                    }
                };
            }

            return new GetUserPhotoByNameQueryResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserPhotoReceivedSuccessfully),
                Body = new() { Path = await _profileService.GetUserPhotoAsync(request.TargetUsername, cancellationToken) }
            };
        }
    }
}
