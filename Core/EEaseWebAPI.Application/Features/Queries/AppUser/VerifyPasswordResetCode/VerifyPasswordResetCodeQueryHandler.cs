using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.ResetPassword;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.VerifyPasswordResetCode
{
    public class VerifyPasswordResetCodeQueryHandler : IRequestHandler<VerifyPasswordResetCodeQueryRequest, VerifyPasswordResetCodeQueryResponse>
    {

        private readonly IPasswordService _passwordService;
        private readonly IHeaderService _headerService;

        public VerifyPasswordResetCodeQueryHandler(IPasswordService passwordService, IHeaderService headerService)
        {
            _passwordService = passwordService;
            _headerService = headerService;
        }

        public async Task<VerifyPasswordResetCodeQueryResponse> Handle(VerifyPasswordResetCodeQueryRequest request, CancellationToken cancellationToken)
        {
            var control = await _passwordService.VerifyResetCodeAsync(request.UsernameOrEmail, request.Code, cancellationToken);

            if (control)
            {
                return new VerifyPasswordResetCodeQueryResponse()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                    Body = new() { Message = AppMessages.ResetCodeCorrect }
                };
            }

            throw new ResetPasswordCodeNotCorrectException("Reset password code not correct", (int)StatusEnum.InvalidResetPasswordCode);
        }
    }
}
