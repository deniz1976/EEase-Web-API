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
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public SetUserPhotoCommandHandler(IUserService userService, IHeaderService headerService)
        {
            _userService = userService;
            _headerService = headerService;
        }

        public async Task<SetUserPhotoCommandResponse> Handle(SetUserPhotoCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.PhotoUrl == null || request.Username == null)
            {
                throw new ArgumentNullException("Request or required properties cannot be null.");
            }
            var result = await _userService.SetUserPhoto(request.Username, request.PhotoUrl);

            if (result) 
            {
                return new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.UserPhotoChangedSuccessfully, true, DateTime.UtcNow)
                };

            }

            throw new Exception("Unexpected error occured");
        }
    }
}
