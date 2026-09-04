using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommandRequest, ChangePasswordCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IAuthService _authService;

        public ChangePasswordCommandHandler(IHeaderService headerService, IAuthService authService)
        {
            _headerService = headerService;
            _authService = authService;
        }

        public async Task<ChangePasswordCommandResponse> Handle(ChangePasswordCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.username == null || request.oldPassword == null)
                throw new ArgumentNullException(nameof(request));

            var result = await _authService.ChangePassword(request.username, request.oldPassword,request.newPassword);

            return new ChangePasswordCommandResponse()
                {
                    ChangePassword = new MapEntities.ChangePassword.ChangePassword()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                        Body = new MapEntities.ChangePassword.ChangePasswordBody()
                        {
                            message = result
                        }
                    }
                };
            
        }
    }
}
