using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.Abstractions.Token;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.Exceptions.Login;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using EEaseWebAPI.Application.Enums;


namespace EEaseWebAPI.Application.Features.Commands.AppUser.LoginUser
{
    public class LoginUserCommandHandler : IRequestHandler<LoginUserCommandRequest, LoginUserCommandResponse>
    {
        private readonly IAuthService _authenticationService;
        private readonly IHeaderService _headerService;


        public LoginUserCommandHandler(IAuthService authenticationService,IHeaderService headerService) 
        {
            _authenticationService = authenticationService;
            _headerService = headerService;
        }


        public async Task<LoginUserCommandResponse> Handle(LoginUserCommandRequest request, CancellationToken cancellationToken)
        {
            var body = await _authenticationService.LoginAsync(request.UsernameOrEmail,request.Password,3600);


            var response = new LoginUserCommandResponse 
            {
                Login = new Login
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.TokenCreatedSuccessfully),
                    Body = body
                }
            };

            return response;
            
        }
    }
}
