using EEaseWebAPI.Application.Abstractions.Services;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;
using EEaseWebAPI.Application.Resources;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RequestPasswordReset
{
    public class RequestPasswordResetCommandHandler : IRequestHandler<RequestPasswordResetCommandRequest, RequestPasswordResetCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IPasswordService _passwordService;

        public RequestPasswordResetCommandHandler(IHeaderService headerService, IPasswordService passwordService)
        {
            _headerService = headerService;
            _passwordService = passwordService;
        }

        public async Task<RequestPasswordResetCommandResponse> Handle(RequestPasswordResetCommandRequest request, CancellationToken cancellationToken)
        {
            // A code that could not be sent is reported by the service; reaching here means
            // it went out.
            await _passwordService.SendResetCodeAsync(request.EmailOrUsername, cancellationToken);

            return new RequestPasswordResetCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.ResetPasswordCodeSentSuccessfully),
                Body = new MapEntities.RequestPasswordResetBody { Message = AppMessages.ResetCodeSent }
            };
        }
    }
}
