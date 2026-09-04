using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.ResetPassword;
using EEaseWebAPI.Application.Enums;
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

        private readonly IAuthService _authService;
        private readonly IHeaderService _headerService;

        public ResetPasswordCodeCheckQueryHandler(IAuthService authService, IHeaderService headerService) 
        {
            _authService = authService;
            _headerService = headerService;
        }

    
        public async Task<ResetPasswordCodeCheckQueryResponse> Handle(ResetPasswordCodeCheckQueryRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.code == null || request.usernameOrEmail == null)
                throw new ArgumentNullException(nameof(request));

            var control = await _authService.ResetPasswordCodeCheck(request.code,request.usernameOrEmail);

            if (control) 
            {
                return new ResetPasswordCodeCheckQueryResponse() 
                {
                    ResetPasswordCodeCheck = new MapEntities.ResetPasswordCodeCheck.ResetPasswordCodeCheck
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                        Body = new() { message = "Code is correct." }
                    }
                };
            }

            throw new ResetPasswordCodeNotCorrectException("Reset password code not correct", (int)StatusEnum.InvalidResetPasswordCode);

            
        }
    }
}
