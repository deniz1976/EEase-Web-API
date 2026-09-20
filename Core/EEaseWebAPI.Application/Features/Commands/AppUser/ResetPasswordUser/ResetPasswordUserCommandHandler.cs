using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;
using Microsoft.Extensions.Localization;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser
{
    public class ResetPasswordUserCommandHandler : IRequestHandler<ResetPasswordUserCommandRequest, ResetPasswordUserCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IPasswordService _passwordService;

        private readonly IStringLocalizer<AppMessages> _messages;


        public ResetPasswordUserCommandHandler(IHeaderService headerService, IPasswordService passwordService,

            IStringLocalizer<AppMessages> messages)

        {
            _headerService = headerService;
            _passwordService = passwordService;

            _messages = messages;
        }

        public async Task<ResetPasswordUserCommandResponse> Handle(ResetPasswordUserCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.EmailOrUsername == null)
                throw new ArgumentNullException(nameof(request));

            var control = await _passwordService.SendResetCodeAsync(request.EmailOrUsername);

            if (control)
            {
                return new ResetPasswordUserCommandResponse()
                {
                    resetPassword = new MapEntities.ResetPassword()
                    {
                        Header = _headerService.HeaderCreate((int)StatusEnum.ResetPasswordCodeSentSuccessfully),
                        Body = new MapEntities.ResetPasswordBody()
                        {
                            message = _messages["ResetCodeSent"]
                        }
                    }
                };
            }

            throw new Exception("Unexpected error occured");
        }
    }
}
