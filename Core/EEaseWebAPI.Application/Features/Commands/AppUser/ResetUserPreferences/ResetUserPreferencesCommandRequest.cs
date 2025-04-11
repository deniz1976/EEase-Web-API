using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetUserPreferences
{
    public class ResetUserPreferencesCommandRequest : IRequest<ResetUserPreferencesCommandResponse>
    {
        public string? Username { get; set; }
    }
} 