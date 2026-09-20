using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferences
{
    public class UpdateUserPreferencesCommandRequest : IRequest<UpdateUserPreferencesCommandResponse>
    {
        public string Message { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
