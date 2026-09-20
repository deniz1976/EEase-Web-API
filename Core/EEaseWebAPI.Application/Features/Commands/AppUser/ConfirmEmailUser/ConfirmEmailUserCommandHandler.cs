using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ConfirmEmailUser
{
    public class ConfirmEmailUserCommandHandler : IRequestHandler<ConfirmEmailUserCommandRequest, ConfirmEmailUserCommandResponse>
    {

        private readonly IUserRegistrationService _registrationService;
        private readonly IHeaderService _headerService;

        public ConfirmEmailUserCommandHandler(IUserRegistrationService registrationService,IHeaderService headerService)
        {
            _headerService = headerService;
            _registrationService = registrationService;
        }

        public async Task<ConfirmEmailUserCommandResponse> Handle(ConfirmEmailUserCommandRequest request, CancellationToken cancellationToken)
        {
            var result = await _registrationService.EmailConfirm(request.Code, request.EmailOrUsername);

            return new ConfirmEmailUserCommandResponse
            {
                Header = _headerService.HeaderCreate((int)StatusEnum.EmailConfirmed),
                Body = new()
                {
                    Result = result
                }
            };

        }
    }
}
