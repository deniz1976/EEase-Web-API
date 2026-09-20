using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.DTOs.User;
using EEaseWebAPI.Application.Features.Commands.AppUser.ConfirmEmailUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.CreateUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.DeleteUserWithCode;
using EEaseWebAPI.Application.Features.Commands.AppUser.ResetUserPreferences;
using EEaseWebAPI.Application.Features.Commands.AppUser.SendVerificationCodeAgain;
using EEaseWebAPI.Application.Features.Commands.AppUser.SetUserPhoto;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUser;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCountry;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserCurrency;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferences;
using EEaseWebAPI.Application.Features.Commands.AppUser.UpdateUserPreferencesWithTopics;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailConfirmed;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckEmailIsInUse;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserCurrency;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfo;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoById;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserInfoByName;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhoto;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPhotoByName;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserPreferenceDescriptions;
using EEaseWebAPI.Application.Features.Queries.AppUser.SearchUsers;
using EEaseWebAPI.Application.Features.Queries.AppUser.StatusCheck;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EEaseWebAPI.API.Controllers
{
    /// <summary>
    /// Travellers. Everything the caller owns hangs off <c>me</c>: a literal segment beats
    /// <c>{username}</c> in routing, so a traveller who registers as "me" cannot shadow it.
    /// </summary>
    [Route("api/users")]
    [ApiController]
    public class UsersController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [ProducesResponseType(typeof(CreateUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> CreateUser(
            [FromBody] CreateUserCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SearchUsersQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchUsers(
            [FromQuery] string search, CancellationToken cancellationToken)
        {
            var request = new SearchUsersQueryRequest { SearchTerm = search };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Whether an address can still be registered with.</summary>
        [HttpGet("email-availability")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CheckEmailIsInUseQueryResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> CheckEmailIsInUse(
            [FromQuery] CheckEmailIsInUseQueryRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Sends another verification code to an address that has not confirmed yet.</summary>
        [HttpPost("email-verifications")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SendVerificationCodeCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> SendVerificationCodeAgain(
            [FromBody] SendVerificationCodeCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("email-confirmation")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CheckEmailConfirmedQueryResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> CheckEmailConfirmed(
            [FromQuery] CheckEmailConfirmedQueryRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("email-confirmation")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ConfirmEmailUserCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> ConfirmEmail(
            [FromBody] ConfirmEmailUserCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("me")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
        {
            var request = new GetUserInfoQueryRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Changes the fields that were sent and leaves the rest alone, which is what every
        /// rule behind it already allowed for.
        /// </summary>
        [HttpPatch("me")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUser(
            [FromBody] UpdateUserDTO updateUserDTO, CancellationToken cancellationToken)
        {
            var request = new UpdateUserCommandRequest
            {
                Username = updateUserDTO.Username,
                Name = updateUserDTO.Name,
                Surname = updateUserDTO.Surname,
                BornDate = updateUserDTO.BornDate,
                Gender = updateUserDTO.Gender,
                Bio = updateUserDTO.Bio,
                User = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Asks for the account to be closed, which sends a code rather than closing it.
        /// Asking again while one is pending calls the deletion off.
        /// </summary>
        [HttpPost("me/deletion-request")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DeleteUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> RequestAccountDeletion(CancellationToken cancellationToken)
        {
            var request = new DeleteUserCommandRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("me")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DeleteUserWithCodeCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> DeleteAccount(
            [FromBody] DeleteAccountWithCode code, CancellationToken cancellationToken)
        {
            var request = new DeleteUserWithCodeCommandRequest
            {
                Username = CurrentUsername,
                Code = code.Code
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("me/status")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(StatusCheckQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetAccountStatus(CancellationToken cancellationToken)
        {
            var request = new StatusCheckQueryRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("me/photo")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPhotoQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPhoto(CancellationToken cancellationToken)
        {
            var request = new GetUserPhotoQueryRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("me/photo")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SetUserPhotoCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SetUserPhoto(
            [FromBody] UserPhoto photo, CancellationToken cancellationToken)
        {
            var request = new SetUserPhotoCommandRequest
            {
                Username = CurrentUsername,
                PhotoUrl = photo.PhotoUrl
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("me/currency")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserCurrencyQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserCurrency(CancellationToken cancellationToken)
        {
            var request = new GetUserCurrencyQueryRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("me/currency")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCurrencyCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserCurrency(
            [FromBody] UserCurrency currency, CancellationToken cancellationToken)
        {
            var request = new UpdateUserCurrencyCommandRequest
            {
                Username = CurrentUsername,
                CurrencyCode = currency.CurrencyCode
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("me/country")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCountryCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserCountry(
            [FromBody] UserCountry country, CancellationToken cancellationToken)
        {
            var request = new UpdateUserCountryCommandRequest
            {
                Country = country.Country,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("me/preferences")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPreferenceDescriptionsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPreferences(CancellationToken cancellationToken)
        {
            var request = new GetUserPreferenceDescriptionsQueryRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Sets the preferences from a sentence the traveller wrote about themselves.</summary>
        [HttpPut("me/preferences")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserPreferencesCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserPreferences(
            [FromBody] PreferenceMessage message, CancellationToken cancellationToken)
        {
            var request = new UpdateUserPreferencesCommandRequest
            {
                Message = message.Message,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("me/preferences")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetUserPreferencesCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetUserPreferences(CancellationToken cancellationToken)
        {
            var request = new ResetUserPreferencesCommandRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Sets the same preferences by naming topics instead of writing a sentence.</summary>
        [HttpPut("me/preferences/topics")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserPreferencesWithTopicsCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserPreferencesWithTopics(
            [FromBody] List<string> topics, CancellationToken cancellationToken)
        {
            var request = new UpdateUserPreferencesWithTopicsCommandRequest
            {
                Username = CurrentUsername,
                Topics = topics
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{username}")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoByNameQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfoByName(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var request = new GetUserInfoByNameQueryRequest
            {
                Username = CurrentUsername,
                TargetUsername = username
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{username}/photo")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPhotoByNameQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPhotoByName(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var request = new GetUserPhotoByNameQueryRequest
            {
                Username = CurrentUsername,
                TargetUsername = username
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// The same traveller, found by the id the database gave them rather than the name
        /// they chose. The extra segment keeps it out of the way of a username.
        /// </summary>
        [HttpGet("by-id/{userId}")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoByIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfoById(
            [FromRoute] string userId, CancellationToken cancellationToken)
        {
            var request = new GetUserInfoByIdQueryRequest
            {
                Username = CurrentUsername,
                UserId = userId
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
