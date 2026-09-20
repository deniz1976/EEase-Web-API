using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword;
using EEaseWebAPI.Application.MapEntities.ResetPasswordWithCode;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser
{
    public class ResetPasswordCommandHandler : IRequestHandler<ResetPasswordCommandRequest, ResetPasswordCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IPasswordService _passwordService;

        public ResetPasswordCommandHandler(IHeaderService headerService, IPasswordService passwordService)
        {
            _headerService = headerService;
            _passwordService = passwordService;
        }

        public async Task<ResetPasswordCommandResponse> Handle(ResetPasswordCommandRequest request, CancellationToken cancellationToken)
        {

            await _passwordService.ResetPasswordAsync(request.UsernameOrEmail, request.Code, request.NewPassword);

            return new ResetPasswordCommandResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                Body = new ResetPasswordWithCodeBody() { message = AppMessages.PasswordChanged}
            };
        }
    }
}
