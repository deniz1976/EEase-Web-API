using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferencesWithTopics
{
    public class UpdateUserPreferencesWithTopicsCommandHandler : IRequestHandler<UpdateUserPreferencesWithTopicsCommandRequest, UpdateUserPreferencesWithTopicsCommandResponse>
    {
        private readonly IUserPreferenceService _preferenceService;
        private readonly IHeaderService _headerService;

        public UpdateUserPreferencesWithTopicsCommandHandler(IUserPreferenceService preferenceService, IHeaderService headerService)
        {
            _preferenceService = preferenceService;
            _headerService = headerService;
        }

        public async Task<UpdateUserPreferencesWithTopicsCommandResponse> Handle(UpdateUserPreferencesWithTopicsCommandRequest request, CancellationToken cancellationToken)
        {
            await _preferenceService.SetFromTopicsAsync(request.Username, request.Topics, cancellationToken);

            return new UpdateUserPreferencesWithTopicsCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.PreferencesUpdatedSuccessfully),
                Body = new UpdateUserPreferencesWithTopicsCommandResponseBody
                {
                    Message = AppMessages.PreferencesUpdatedWithTopics
                }
            };
        }
    }
}
