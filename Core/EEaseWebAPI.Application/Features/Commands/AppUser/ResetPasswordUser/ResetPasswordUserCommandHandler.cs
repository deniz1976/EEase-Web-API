using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser
{
    public class ResetPasswordUserCommandHandler : IRequestHandler<ResetPasswordUserCommandRequest, ResetPasswordUserCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IPasswordService _passwordService;

        public ResetPasswordUserCommandHandler(IHeaderService headerService, IPasswordService passwordService)
        {
            _headerService = headerService;
            _passwordService = passwordService;
        }

        public async Task<ResetPasswordUserCommandResponse> Handle(ResetPasswordUserCommandRequest request, CancellationToken cancellationToken)
        {
            // A code that could not be sent is reported by the service; reaching here means
            // it went out.
            await _passwordService.SendResetCodeAsync(request.EmailOrUsername, cancellationToken);

            return new ResetPasswordUserCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.ResetPasswordCodeSentSuccessfully),
                Body = new MapEntities.ResetPasswordBody { Message = AppMessages.ResetCodeSent }
            };
        }
    }
}
