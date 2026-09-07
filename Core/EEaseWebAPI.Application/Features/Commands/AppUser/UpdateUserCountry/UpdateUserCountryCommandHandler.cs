using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUserCountry;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry
{
    public class UpdateUserCountryCommandHandler : IRequestHandler<UpdateUserCountryCommandRequest, UpdateUserCountryCommandResponse>
    {
        private readonly IUserProfileService _profileService;
        private readonly IHeaderService _headerService;

        public UpdateUserCountryCommandHandler(IUserProfileService profileService, IHeaderService headerService)
        {
            _profileService = profileService;
            _headerService = headerService;
        }

        public async Task<UpdateUserCountryCommandResponse> Handle(UpdateUserCountryCommandRequest request, CancellationToken cancellationToken)
        {
            try
            {
                bool result = await _profileService.UpdateUserCountry(request.Username, request.Country);

                return new UpdateUserCountryCommandResponse
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UpdateUserCountrySuccess),
                    Body = new UpdateUserCountryCommandResponseBody()
                };
            }
            catch (InvalidCountryException ex)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to update user country: {ex.Message}", ex);
            }
        }
    }
}
