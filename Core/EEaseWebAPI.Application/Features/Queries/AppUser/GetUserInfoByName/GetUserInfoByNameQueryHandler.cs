using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoByName
{
    public class GetUserInfoByNameQueryHandler : IRequestHandler<GetUserInfoByNameQueryRequest, GetUserInfoByNameQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;
        private readonly IUserPreferenceService _preferenceService;

        public GetUserInfoByNameQueryHandler(
            IHeaderService headerService,
            IUserProfileService profileService,
            IUserPreferenceService preferenceService)
        {
            _headerService = headerService;
            _profileService = profileService;
            _preferenceService = preferenceService;
        }

        public async Task<GetUserInfoByNameQueryResponse> Handle(
            GetUserInfoByNameQueryRequest request, CancellationToken cancellationToken)
        {
            var (userInfo, visibility) = await _profileService.GetUserInfoByNameAsync(
                request.Username, request.TargetUsername, cancellationToken);

            var (headerCode, body) = await UserProfileView.BuildAsync(
                userInfo, visibility, request.Username, _preferenceService, cancellationToken);

            return new GetUserInfoByNameQueryResponse
            {
                Header = _headerService.HeaderCreate(headerCode),
                Body = body
            };
        }
    }
}
