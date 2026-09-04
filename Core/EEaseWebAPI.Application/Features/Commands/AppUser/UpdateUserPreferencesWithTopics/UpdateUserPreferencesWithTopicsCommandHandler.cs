using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferencesWithTopics
{
    public class UpdateUserPreferencesWithTopicsCommandHandler : IRequestHandler<UpdateUserPreferencesWithTopicsCommandRequest, UpdateUserPreferencesWithTopicsCommandResponse>
    {
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public UpdateUserPreferencesWithTopicsCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<UpdateUserPreferencesWithTopicsCommandResponse> Handle(UpdateUserPreferencesWithTopicsCommandRequest request, CancellationToken cancellationToken)
        {
            await _userService.UpdateUserPreferencesWithTopics(request.Username, request.Topics);

            return new UpdateUserPreferencesWithTopicsCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.PreferencesUpdatedSuccessfully),
                Body = new UpdateUserPreferencesWithTopicsCommandResponseBody
                {
                    Message = "User preferences updated successfully with selected topics"
                }
            };
        }
    }
}
