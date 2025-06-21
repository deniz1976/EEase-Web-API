using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Exceptions.UpdateUser;
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
        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;
        private readonly IAuthService _authService;


        public UpdateUserCommandHandler(UserManager<Domain.Entities.Identity.AppUser> userManager,IUserService userService,IHeaderService headerService,
            IAuthService authService)
        {
            _userManager = userManager;
            _userService = userService;
            _headerService = headerService;
            _authService = authService;
        }

        public async Task<UpdateUserCommandResponse> Handle(UpdateUserCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null)
                throw new ArgumentNullException(nameof(request));
            if (request.user == null)
                throw new UserNotFoundException("User not found",(int)StatusEnum.UserNotFound);

            var result = await _userService.UpdateUser(request);
            

            if(result && request.Username != null)
            {

                return new UpdateUserCommandResponse()
                {
                    UpdateUser = new MapEntities.UpdateUser.UpdateUser()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.UserUpdatedSuccessfully),
                        Body = new MapEntities.UpdateUser.UpdateUserBody()
                        {
                            message = "User updated and new token created please change your token.",
                            newToken = await _authService.UpdateUserGetNewToken(request.Username)
                        }
                    }
                };
            }

            if(result && request.Username == null)
            {
                return new UpdateUserCommandResponse()
                {
                    UpdateUser = new MapEntities.UpdateUser.UpdateUser()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.UserUpdatedSuccessfully),
                        Body = new MapEntities.UpdateUser.UpdateUserBody()
                        {
                            message = "User updated and old token still can be used"
                        }
                    }
                };
            }

            throw new Exception("An unexpected error occured");


        }
    }
}
