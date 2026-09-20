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
        private readonly IPasswordService _passwordService;

        public ChangePasswordCommandHandler(IHeaderService headerService, IPasswordService passwordService)
        {
            _headerService = headerService;
            _passwordService = passwordService;
        }

        public async Task<ChangePasswordCommandResponse> Handle(ChangePasswordCommandRequest request, CancellationToken cancellationToken)
        {
var result = await _passwordService.ChangePasswordAsync(request.Username, request.OldPassword,request.NewPassword);

            return new ChangePasswordCommandResponse()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                    Body = new MapEntities.ChangePassword.ChangePasswordBody()
                    {
                        Message = result
                    }
                };

        }
    }
}
