using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.UpdateUserPreferences;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferences
{
    public class UpdateUserPreferencesCommandHandler : IRequestHandler<UpdateUserPreferencesCommandRequest, UpdateUserPreferencesCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public UpdateUserPreferencesCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<UpdateUserPreferencesCommandResponse> Handle(UpdateUserPreferencesCommandRequest request, CancellationToken cancellationToken)
        {
            await _userService.UpdateUserPreferences(request.Username, request.Message);

            return new UpdateUserPreferencesCommandResponse
            {
                response = new UpdateUserPreferencesResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PreferencesUpdatedSuccessfully),
                    Body = new UpdateUserPreferencesBody
                    {
                        message = "User preferences updated successfully"
                    }
                }
            };
        }
    }
} 