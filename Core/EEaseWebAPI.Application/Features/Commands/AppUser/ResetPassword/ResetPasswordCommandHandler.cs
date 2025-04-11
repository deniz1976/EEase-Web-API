using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword;
using EEaseWebAPI.Application.MapEntities.ResetPasswordWithCode;
using EEaseWebAPI.Application.Enums;
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
        private readonly IAuthService _authService;

        public ResetPasswordCommandHandler(IHeaderService headerService, IAuthService authService)
        {
            _headerService = headerService;
            _authService = authService;
        }

        public async Task<ResetPasswordCommandResponse> Handle(ResetPasswordCommandRequest request, CancellationToken cancellationToken)
        {

         if(request == null ||request.code == null || request.newPassword == null)
                throw new ArgumentNullException(nameof(request));


         await _authService.ResetPasswordWithCode(request.code, request.newPassword);
           
        
            return new ResetPasswordCommandResponse() 
            {
                ResetPasswordWithCode = new ResetPasswordWithCode()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.PasswordChangedSuccessfully),
                    Body = new ResetPasswordWithCodeBody() { message = "Password changed succesfully."}
                }
            };
            
            
        }
    }
}
