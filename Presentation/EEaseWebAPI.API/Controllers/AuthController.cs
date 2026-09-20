using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ChangePassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.LoginUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.RefreshTokenLoginUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser;
using EEaseWebAPI.Application.Features.Queries;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailIsInUse;
using EEaseWebAPI.Application.Features.Queries.AppUser.ResetPasswordCodeCheck;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(LoginUserCommandResponse),StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> Login([FromBody] LoginUserCommandRequest request, CancellationToken cancellationToken)
        {
            LoginUserCommandResponse loginUserCommandResponse = await _mediator.Send(request, cancellationToken);
            return Ok(loginUserCommandResponse);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(RefreshTokenLoginUserCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> RefreshTokenLogin([FromBody] RefreshTokenLoginUserCommandRequest request, CancellationToken cancellationToken)
        {
            RefreshTokenLoginUserCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetPasswordUserCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordUserCommandRequest request, CancellationToken cancellationToken)
        {
            ResetPasswordUserCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetPasswordCodeCheckQueryResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> ResetPasswordCodeCheck([FromBody] ResetPasswordCodeCheckQueryRequest request, CancellationToken cancellationToken)
        {
            ResetPasswordCodeCheckQueryResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetPasswordCommandResponse),StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> ResetPasswordWithCode([FromBody] ResetPasswordCommandRequest request, CancellationToken cancellationToken)
        {
            ResetPasswordCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ChangePasswordCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO changePasswordDTO, CancellationToken cancellationToken)
        {
            ChangePasswordCommandRequest request = new ChangePasswordCommandRequest() { Username = CurrentUsername,OldPassword = changePasswordDTO.OldPassword, NewPassword = changePasswordDTO.NewPassword};
            ChangePasswordCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CheckEmailIsInUseQueryResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> CheckEmailIsInUse([FromQuery] CheckEmailIsInUseQueryRequest request, CancellationToken cancellationToken)
        {
            CheckEmailIsInUseQueryResponse checkEmailIsInUseQueryResponse = await _mediator.Send(request, cancellationToken);
            return Ok(checkEmailIsInUseQueryResponse);
        }
    }
}
