using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Application.DTOs;
using EEaseWebAPI.Application.MapEntities.Login;
using EEaseWebAPI.Application.MapEntities.RefreshTokenLogin;
using EEaseWebAPI.Application.Enums;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EEaseWebAPI.Application.Features.Commands.AppUser.RefreshTokenLoginUser
{
    public class RefreshTokenLoginUserCommandHandler : IRequestHandler<RefreshTokenLoginUserCommandRequest, RefreshTokenLoginUserCommandResponse>
    {
        private readonly IHeaderService _headerService;
        private readonly IAuthService _authService;

        public RefreshTokenLoginUserCommandHandler(IHeaderService headerService, IAuthService authService)
        {
            _headerService = headerService;
            _authService = authService;
        }

        public async Task<RefreshTokenLoginUserCommandResponse> Handle(RefreshTokenLoginUserCommandRequest request, CancellationToken cancellationToken)
        {
            if (request == null || request.RefreshToken == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            RefreshTokenLoginBody token = await _authService.RefreshTokenLoginAsync(request.RefreshToken);

            return new RefreshTokenLoginUserCommandResponse 
            {
                RefreshTokenLogin = new MapEntities.RefreshTokenLogin.RefreshTokenLogin 
                {
                    Header = _headerService.HeaderCreate((int)StatusEnum.TokenRefreshedSuccessfully),
                    Body = new MapEntities.RefreshTokenLogin.RefreshTokenLoginBody() 
                    {
                        Token = token.Token,
                        warning = token.warning
                    }
                }
            };

        }
    }
}
