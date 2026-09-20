using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ChangePassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.LoginUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.RefreshTokenLoginUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPassword;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetPasswordUser;
using EEaseWebAPI.Application.Features.Queries;
using EEaseWebAPI.Application.Features.Queries.AppUser.ResetPasswordCodeCheck;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EEaseWebAPI.API.Controllers
{
    /// <summary>
    /// Signing in and getting back in. These are the one place in the API where a path
    /// names an act rather than a thing: there is no useful noun for "prove who you are".
    /// </summary>
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginUserCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> Login(
            [FromBody] LoginUserCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(typeof(RefreshTokenLoginUserCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> RefreshTokenLogin(
            [FromBody] RefreshTokenLoginUserCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Changing a password you still know, which needs the old one.</summary>
        [HttpPut("password")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ChangePasswordCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> ChangePassword(
            [FromBody] ChangePasswordDTO changePasswordDTO, CancellationToken cancellationToken)
        {
            var request = new ChangePasswordCommandRequest
            {
                Username = CurrentUsername,
                OldPassword = changePasswordDTO.OldPassword,
                NewPassword = changePasswordDTO.NewPassword
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Starts a reset for a password nobody remembers, by sending a code.</summary>
        [HttpPost("password-resets")]
        [ProducesResponseType(typeof(ResetPasswordUserCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> RequestPasswordReset(
            [FromBody] ResetPasswordUserCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("password-resets/verify")]
        [ProducesResponseType(typeof(ResetPasswordCodeCheckQueryResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> VerifyPasswordResetCode(
            [FromBody] ResetPasswordCodeCheckQueryRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("password-resets/complete")]
        [ProducesResponseType(typeof(ResetPasswordCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> CompletePasswordReset(
            [FromBody] ResetPasswordCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
