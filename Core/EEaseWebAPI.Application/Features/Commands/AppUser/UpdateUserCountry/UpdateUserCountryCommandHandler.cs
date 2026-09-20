using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUserCountry;
using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry
{
    public class UpdateUserCountryCommandHandler : IRequestHandler<UpdateUserCountryCommandRequest, UpdateUserCountryCommandResponse>
    {
        private readonly IUserProfileService _profileService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public UpdateUserCountryCommandHandler(IUserProfileService profileService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _profileService = profileService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<UpdateUserCountryCommandResponse> Handle(UpdateUserCountryCommandRequest request, CancellationToken cancellationToken)
        {
            bool result = await _profileService.UpdateUserCountry(request.Username, request.Country);

            return new UpdateUserCountryCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UpdateUserCountrySuccess),
                Body = new UpdateUserCountryCommandResponseBody
                {
                    Message = _messages["CountryUpdated"]
                }
            };
        }
    }
}
