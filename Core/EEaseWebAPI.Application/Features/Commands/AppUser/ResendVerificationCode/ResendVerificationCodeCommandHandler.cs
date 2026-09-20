using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.Login;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.ResendVerificationCode
{
    public class ResendVerificationCodeCommandHandler : IRequestHandler<ResendVerificationCodeCommandRequest, ResendVerificationCodeCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserRegistrationService _registrationService;
        private static string success = "Code sent to mail successfully.";

        public ResendVerificationCodeCommandHandler(IHeaderService headerService, IUserRegistrationService registrationService)
        {
            _headerService = headerService;
            _registrationService = registrationService;
        }

        public async Task<ResendVerificationCodeCommandResponse> Handle(ResendVerificationCodeCommandRequest request, CancellationToken cancellationToken)
        {
            if (request.Email == null) { throw new UserNotFoundException("User Not Found",7); }

            var result = await _registrationService.SendVerificationEmailAgain(request.Email, cancellationToken);
            if (result)
            {
                return new ResendVerificationCodeCommandResponse()
                {
                    Header = _headerService.HeaderCreate(98),
                    Body = new ResendVerificationCodeBody()
                    {
                        Message = success,
                        Success = result
                    }
                };
            }

            throw new NotImplementedException();
        }
    }
}
