using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoById
{
    public class GetUserInfoByIdQueryHandler : IRequestHandler<GetUserInfoByIdQueryRequest, GetUserInfoByIdQueryResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserProfileService _profileService;
        private readonly IUserPreferenceService _preferenceService;

        public GetUserInfoByIdQueryHandler(
            IHeaderService headerService,
            IUserProfileService profileService,
            IUserPreferenceService preferenceService)
        {
            _headerService = headerService;
            _profileService = profileService;
            _preferenceService = preferenceService;
        }

        public async Task<GetUserInfoByIdQueryResponse> Handle(
            GetUserInfoByIdQueryRequest request, CancellationToken cancellationToken)
        {
            var (userInfo, visibility) = await _profileService.GetUserInfoByIdAsync(
                request.Username, request.UserId, cancellationToken);

            var (headerCode, body) = await UserProfileView.BuildAsync(
                userInfo, visibility, request.Username, _preferenceService, cancellationToken);

            return new GetUserInfoByIdQueryResponse
            {
                Header = _headerService.HeaderCreate(headerCode),
                Body = body
            };
        }
    }
}
