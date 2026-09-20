using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
using EEaseWebAPI.Application.Resources;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommandRequest, UpdateUserCommandResponse>
    {
        private readonly UserManager<Domain.Entities.Identity.AppUser> _userManager;
        private readonly IUserProfileService _profileService;
        private readonly IHeaderService _headerService;
        private readonly IAuthService _authService;

        public UpdateUserCommandHandler(UserManager<Domain.Entities.Identity.AppUser> userManager,IUserProfileService profileService,IHeaderService headerService,
            IAuthService authService)
        {
            _userManager = userManager;
            _profileService = profileService;
            _headerService = headerService;
            _authService = authService;
        }

        public async Task<UpdateUserCommandResponse> Handle(UpdateUserCommandRequest request, CancellationToken cancellationToken)
        {
            if (request.User == null)
                throw new UserNotFoundException("User not found",(int)StatusEnum.UserNotFound);

            // An update that did not happen leaves the service by throwing, so there is no
            // third outcome to answer with an empty 500.
            await _profileService.UpdateUser(request, cancellationToken);

            // A new username means the old token names somebody who no longer exists.
            var body = request.Username == null
                ? new MapEntities.UpdateUser.UpdateUserBody { Message = AppMessages.UserUpdated }
                : new MapEntities.UpdateUser.UpdateUserBody
                {
                    Message = AppMessages.UserUpdatedWithNewToken,
                    NewToken = await _authService.UpdateUserGetNewToken(request.Username, cancellationToken)
                };

            return new UpdateUserCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.UserUpdatedSuccessfully),
                Body = body
            };
        }
    }
}
