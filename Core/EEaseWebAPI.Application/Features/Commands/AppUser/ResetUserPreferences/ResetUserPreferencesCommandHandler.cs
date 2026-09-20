using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.ResetUserPreferences;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.ResetUserPreferences;
using EEaseWebAPI.Application.Resources;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetUserPreferences
{
    public class ResetUserPreferencesCommandHandler : IRequestHandler<ResetUserPreferencesCommandRequest, ResetUserPreferencesCommandResponse>
    {
        private readonly IUserPreferenceService _preferenceService;
        private readonly IHeaderService _headerService;

        public ResetUserPreferencesCommandHandler(IUserPreferenceService preferenceService, IHeaderService headerService)
        {
            _preferenceService = preferenceService;
            _headerService = headerService;
        }

        public async Task<ResetUserPreferencesCommandResponse> Handle(ResetUserPreferencesCommandRequest request, CancellationToken cancellationToken)
        {
            await _preferenceService.ResetAsync(request.Username);

            return new ResetUserPreferencesCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.PreferencesResetSuccessfully),
                Body = new ResetUserPreferencesBody
                {
                    Message = AppMessages.PreferencesReset
                }
            };
        }
    }
}
