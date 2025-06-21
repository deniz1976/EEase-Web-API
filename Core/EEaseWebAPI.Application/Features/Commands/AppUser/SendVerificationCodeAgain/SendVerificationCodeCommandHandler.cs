using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Exceptions.Login;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.SendVerificationCodeAgain
{
    public class SendVerificationCodeCommandHandler : IRequestHandler<SendVerificationCodeCommandRequest, SendVerificationCodeCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IUserService _userService;
        private static string success = "Code sent to mail successfully.";

        public SendVerificationCodeCommandHandler(IHeaderService headerService, IUserService userService)
        {
            _headerService = headerService;
            _userService = userService;
        }

        public async Task<SendVerificationCodeCommandResponse> Handle(SendVerificationCodeCommandRequest request, CancellationToken cancellationToken)
        {
            if (request.email == null) { throw new UserNotFoundException("User Not Found",7); }

            var result = await _userService.SendVerificationEmailAgain(request.email);
            if (result) 
            {
                return new SendVerificationCodeCommandResponse() 
                {
                    Header = _headerService.HeaderCreate(98),
                    Body = new SendVerificationCodeBody() 
                    {
                        message = success,
                        success = result
                    }
                };
            }

            throw new NotImplementedException();
        }
    }
}
