using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.UpdateUserPreferences;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferences
{
    public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommandRequest, UpdateUserPreferencesCommandResponse>
    {
        private readonly IUserPreferenceService _preferenceService;
        private readonly IHeaderService _headerService;

        public UpdateUserPreferencesCommandHandler(IUserPreferenceService preferenceService, IHeaderService headerService)
        {
            _preferenceService = preferenceService;
            _headerService = headerService;
        }

        public async Task<UpdateUserPreferencesCommandResponse> Handle(UpdateUserPreferencesCommandRequest request, CancellationToken cancellationToken)
        {
            await _preferenceService.SetFromMessageAsync(request.Username, request.Message);

            return new UpdateUserPreferencesCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.PreferencesUpdatedSuccessfully),
                Body = new UpdateUserPreferencesBody
                {
                    Message = AppMessages.PreferencesUpdated
                }
            };
        }
    }
}
