using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhotoByName
{
    public class GetUserPhotoByNameQueryHandler : IRequestHandler<GetUserPhotoByNameQueryRequest, GetUserPhotoByNameQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;
        private readonly IFriendshipService _friendshipService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public GetUserPhotoByNameQueryHandler(
            IHeaderService headerService,
            IUserProfileService profileService,
            IFriendshipService friendshipService,

            IStringLocalizer<AppMessages> messages)

        {
            _headerService = headerService;
            _profileService = profileService;
            _friendshipService = friendshipService;

            _messages = messages;
        }

        public async Task<GetUserPhotoByNameQueryResponse> Handle(GetUserPhotoByNameQueryRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.username == null || request.targetUsername == null)
                throw new ArgumentNullException(nameof(request));

            var isSelf = request.username == request.targetUsername;

            if (!isSelf && !await _friendshipService.AreFriendsAsync(request.username, request.targetUsername))
            {
                return new GetUserPhotoByNameQueryResponse()
                {
                    response = new()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.NotFriends),
                        Body = new()
                        {
                            path = null,
                            errorMessage = _messages["NotFriendsPhoto"]
                        }
                    }
                };
            }

            return new GetUserPhotoByNameQueryResponse()
            {
                response = new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserPhotoReceivedSuccessfully),
                    Body = new() { path = await _profileService.GetUserPhotoAsync(request.targetUsername) }
                }
            };
        }
    }
}
