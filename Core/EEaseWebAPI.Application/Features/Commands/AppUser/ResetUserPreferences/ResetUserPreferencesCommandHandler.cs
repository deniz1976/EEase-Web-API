using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.ResetUserPreferences;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.ResetUserPreferences;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetUserPreferences
{
    public class ResetUserPreferencesCommandHandler : IRequestHandler<ResetUserPreferencesCommandRequest, ResetUserPreferencesCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public ResetUserPreferencesCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<ResetUserPreferencesCommandResponse> Handle(ResetUserPreferencesCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                await _userService.ResetUserPreferences(request.Username);

                return new ResetUserPreferencesCommandResponse
                {
                    Response = new ResetUserPreferencesResponse
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.PreferencesResetSuccessfully),
                        Body = new ResetUserPreferencesBody
                        {
                            Message = "User preferences have been reset successfully"
                        }
                    }
                };
            }
            catch (UserPreferencesNotFoundException ex)
            {
                throw;
            }
            catch (ResetUserPreferencesFailedException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new ResetUserPreferencesFailedException("An unexpected error occurred while resetting user preferences", ex);
            }
        }
    }
} 