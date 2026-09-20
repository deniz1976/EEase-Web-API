using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.ResetPassword;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Queries.AppUser.ResetPasswordCodeCheck
{
    public class ResetPasswordCodeCheckQueryHandler : IRequestHandler<ResetPasswordCodeCheckQueryRequest, ResetPasswordCodeCheckQueryResponse>
    {

        private readonly IPasswordService _passwordService;
        private readonly IHeaderService _headerService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public ResetPasswordCodeCheckQueryHandler(IPasswordService passwordService, IHeaderService headerService,

            IStringLocalizer<AppMessages> messages)

        {
            _passwordService = passwordService;
            _headerService = headerService;

            _messages = messages;
        }

        public async Task<ResetPasswordCodeCheckQueryResponse> Handle(ResetPasswordCodeCheckQueryRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.code == null || request.usernameOrEmail == null)
                throw new ArgumentNullException(nameof(request));

            var control = await _passwordService.VerifyResetCodeAsync(request.usernameOrEmail, request.code);

            if (control)
            {
                return new ResetPasswordCodeCheckQueryResponse()
                {
                    ResetPasswordCodeCheck = new MapEntities.ResetPasswordCodeCheck.ResetPasswordCodeCheck
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                        Body = new() { message = _messages["ResetCodeCorrect"] }
                    }
                };
            }

            throw new ResetPasswordCodeNotCorrectException("Reset password code not correct", (int)StatusEnum.InvalidResetPasswordCode);

        }
    }
}
