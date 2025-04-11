using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ChangePassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.LoginUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.RefreshTokenLoginUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser;
using EEaseWebAPI.Application.Features.Queries;
using EEaseWebAPI.Application.Features.Queries.AppUser;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailIsInUse;
using EEaseWebAPI.Application.Features.Queries.AppUser.ResetPasswordCodeCheck;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator) 
        {
            _mediator = mediator;
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(LoginUserCommandResponse),StatusCodes.Status200OK)]

        public async Task<IActionResult> Login(LoginUserCommandRequest request) 
        {
            LoginUserCommandResponse loginUserCommandResponse = await _mediator.Send(request);
            return Ok(loginUserCommandResponse);
        }



        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(RefreshTokenLoginUserCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> RefreshTokenLoginAsync(RefreshTokenLoginUserCommandRequest request) 
        {
            RefreshTokenLoginUserCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }


        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetPasswordUserCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPassword(ResetPasswordUserCommandRequest request)
        {
            ResetPasswordUserCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetPasswordCodeCheckQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPasswordCodeCheck(ResetPasswordCodeCheckQueryRequest request) 
        {
            ResetPasswordCodeCheckQueryResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [ProducesResponseType(typeof(GlobalError),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetPasswordCommandResponse),StatusCodes.Status200OK)]
        public async Task<IActionResult> ResetPasswordWithCode(ResetPasswordCommandRequest request) 
        {
            ResetPasswordCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        
        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ChangePasswordCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO) 
        {
            ChangePasswordCommandRequest request = new ChangePasswordCommandRequest() { username = User.FindFirst(ClaimTypes.Name)?.Value,oldPassword = changePasswordDTO.oldpassword, newPassword = changePasswordDTO.newpassword};
            ChangePasswordCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }


        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(GlobalError),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CheckEmailIsInUseQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmailIsInUse([FromQuery]CheckEmailIsInUseQueryRequest request)
        {
            CheckEmailIsInUseQueryResponse checkEmailIsInUseQueryResponse = await _mediator.Send(request);
            return Ok(checkEmailIsInUseQueryResponse);
        }
    }
}
