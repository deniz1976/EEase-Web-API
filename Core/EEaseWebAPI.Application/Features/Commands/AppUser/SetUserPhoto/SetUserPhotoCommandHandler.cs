using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SetUserPhoto
{
    public class SetUserPhotoCommandHandler : IRequestHandler<SetUserPhotoCommandRequest, SetUserPhotoCommandResponse>
    {
        private readonly IUserProfileService _profileService;
        private readonly IHeaderService _headerService;

        public SetUserPhotoCommandHandler(IUserProfileService profileService, IHeaderService headerService)
        {
            _profileService = profileService;
            _headerService = headerService;
        }

        public async Task<SetUserPhotoCommandResponse> Handle(SetUserPhotoCommandRequest request, CancellationToken cancellationToken)
        {
            await _profileService.SetUserPhoto(request.Username, request.PhotoUrl, cancellationToken);

            return new SetUserPhotoCommandResponse
            {
                Header = _headerService.HeaderCreate(
                    (int)StatusEnum.UserPhotoChangedSuccessfully, true, DateTime.UtcNow),
                Body = new SetUserPhotoCommandResponseBody { PhotoPath = request.PhotoUrl }
            };
        }
    }
}
