using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ConfirmEmail
{
    public class ConfirmEmailCommandHandler : IRequestHandler<ConfirmEmailCommandRequest, ConfirmEmailCommandResponse>
    {
        private readonly IUserRegistrationService _registrationService;
        private readonly IHeaderService _headerService;

        public ConfirmEmailCommandHandler(IUserRegistrationService registrationService,IHeaderService headerService)
        {
            _headerService = headerService;
            _registrationService = registrationService;
        }

        public async Task<ConfirmEmailCommandResponse> Handle(ConfirmEmailCommandRequest request, CancellationToken cancellationToken)
        {
            var result = await _registrationService.EmailConfirm(request.Code, request.EmailOrUsername, cancellationToken);

            return new ConfirmEmailCommandResponse
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
