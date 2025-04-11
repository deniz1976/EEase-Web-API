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

        private readonly IUserService _userService;
        private readonly IHeaderService _headerService;

        public ConfirmEmailUserCommandHandler(IUserService userService,IHeaderService headerService)
        {
            _headerService = headerService;
            _userService = userService;
        }
    
        public async Task<ConfirmEmailUserCommandResponse> Handle(ConfirmEmailUserCommandRequest request, CancellationToken cancellationToken)
        {
            if(request == null || request.code == null || request.emailOrUsername == null )
                throw new ArgumentNullException(nameof(request));

            var result = await _userService.EmailConfirm(request.code, request.emailOrUsername);


            return new ConfirmEmailUserCommandResponse
            {
                confirmEmail = new()
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.EmailConfirmed),
                    Body = new() 
                    {
                        result = result
                    }
                }
            };

        }
    }
}
