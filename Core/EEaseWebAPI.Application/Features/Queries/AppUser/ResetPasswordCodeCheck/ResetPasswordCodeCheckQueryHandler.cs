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

namespace EEaseWebAPI.Application.Features.Queries.AppUser.ResetPasswordCodeCheck
{
    public class ResetPasswordCodeCheckQueryHandler : IRequestHandler<ResetPasswordCodeCheckQueryRequest, ResetPasswordCodeCheckQueryResponse>
    {

        private readonly IPasswordService _passwordService;
        private readonly IHeaderService _headerService;

        public ResetPasswordCodeCheckQueryHandler(IPasswordService passwordService, IHeaderService headerService)
        {
            _passwordService = passwordService;
            _headerService = headerService;
        }

        public async Task<ResetPasswordCodeCheckQueryResponse> Handle(ResetPasswordCodeCheckQueryRequest request, CancellationToken cancellationToken)
        {
            var control = await _passwordService.VerifyResetCodeAsync(request.UsernameOrEmail, request.Code, cancellationToken);

            if (control)
            {
                return new ResetPasswordCodeCheckQueryResponse()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                    Body = new() { Message = AppMessages.ResetCodeCorrect }
                };
            }

            throw new ResetPasswordCodeNotCorrectException("Reset password code not correct", (int)StatusEnum.InvalidResetPasswordCode);
        }
    }
}
