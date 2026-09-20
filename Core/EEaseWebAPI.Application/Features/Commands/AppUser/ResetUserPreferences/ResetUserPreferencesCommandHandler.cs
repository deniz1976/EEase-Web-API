using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.ResetUserPreferences;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.ResetUserPreferences;
using MediatR;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetUserPreferences
{
    public class ResetUserPreferencesCommandHandler : IRequestHandler<ResetUserPreferencesCommandRequest, ResetUserPreferencesCommandResponse>
    {
        private readonly IUserPreferenceService _preferenceService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public ResetUserPreferencesCommandHandler(IUserPreferenceService preferenceService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _preferenceService = preferenceService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<ResetUserPreferencesCommandResponse> Handle(ResetUserPreferencesCommandRequest request, CancellationToken cancellationToken)
        {
            await _preferenceService.ResetAsync(request.Username);

            return new ResetUserPreferencesCommandResponse
            {
                Response = new ResetUserPreferencesResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PreferencesResetSuccessfully),
                    Body = new ResetUserPreferencesBody
                    {
                        Message = _messages["PreferencesReset"]
                    }
                }
            };
        }
    }
}
