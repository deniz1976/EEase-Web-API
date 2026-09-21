using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Features.Commands.AppUser.CompletePasswordReset;
using EEaseWebAPI.Application.MapEntities.CompletePasswordReset;
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
    public class CompletePasswordResetCommandHandler : IRequestHandler<CompletePasswordResetCommandRequest, CompletePasswordResetCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IPasswordService _passwordService;

        public CompletePasswordResetCommandHandler(IHeaderService headerService, IPasswordService passwordService)
        {
            _headerService = headerService;
            _passwordService = passwordService;
        }

        public async Task<CompletePasswordResetCommandResponse> Handle(CompletePasswordResetCommandRequest request, CancellationToken cancellationToken)
        {
            await _passwordService.ResetPasswordAsync(request.UsernameOrEmail, request.Code, request.NewPassword, cancellationToken);

            return new CompletePasswordResetCommandResponse()
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                Body = new CompletePasswordResetBody() { message = AppMessages.PasswordChanged}
            };
        }
    }
}
