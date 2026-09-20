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
using EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics;
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
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.AspNetCore.Mvc;
namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [ProducesResponseType(typeof(CreateUserCommandResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse),StatusCodes.Status500InternalServerError)]
        [HttpPost("[Action]")]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserCommandRequest createUserCommandRequest, CancellationToken cancellationToken)
        {
            CreateUserCommandResponse createUserCommandResponse = await _mediator.Send(createUserCommandRequest, cancellationToken);
            return Ok(createUserCommandResponse);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SendVerificationCodeCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> SendVerificationCodeAgain([FromBody] SendVerificationCodeCommandRequest request, CancellationToken cancellationToken)
        {
           SendVerificationCodeCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);

        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateUserDTO updateUserDTO, CancellationToken cancellationToken)
        {
            UpdateUserCommandRequest updateUserCommandRequest = new UpdateUserCommandRequest()
            {
                Username = updateUserDTO.Username,
                Name = updateUserDTO.Name,
                Surname = updateUserDTO.Surname,
                BornDate = updateUserDTO.BornDate,
                Gender = updateUserDTO.Gender,
                Bio = updateUserDTO.Bio,
                User = CurrentUsername
            };

            UpdateUserCommandResponse updateUserCommandResponse = await _mediator.Send(updateUserCommandRequest, cancellationToken);
            return Ok(updateUserCommandResponse);
        }

        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("[Action]")]
        public async Task<IActionResult> GetUserInfo(CancellationToken cancellationToken)
        {
            var userName = CurrentUsername;
            GetUserInfoQueryRequest request = new() { Username = userName };
            GetUserInfoQueryResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CheckEmailConfirmedQueryResponse),StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> CheckEmailConfirmed([FromQuery] CheckEmailConfirmedQueryRequest request, CancellationToken cancellationToken)
        {
            CheckEmailConfirmedQueryResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ConfirmEmailUserCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> EmailConfirm([FromBody] ConfirmEmailUserCommandRequest request, CancellationToken cancellationToken)
        {
            ConfirmEmailUserCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DeleteUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> DeleteAccount(CancellationToken cancellationToken)
        {

            DeleteUserCommandRequest deleteUserCommandRequest = new DeleteUserCommandRequest()
            { Username=  CurrentUsername };
            DeleteUserCommandResponse response = await _mediator.Send(deleteUserCommandRequest, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DeleteUserWithCodeCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [EnableRateLimiting(RateLimitPolicies.Sensitive)]
        public async Task<IActionResult> DeleteAccountWithCode([FromBody] DeleteAccountWithCode code, CancellationToken cancellationToken)
        {
            DeleteUserWithCodeCommandRequest deleteUserWithCodeCommandRequest = new DeleteUserWithCodeCommandRequest() {Username = CurrentUsername,Code = code.Code };
            DeleteUserWithCodeCommandResponse deleteUserWithCodeCommandResponse = await _mediator.Send(deleteUserWithCodeCommandRequest, cancellationToken);
            return Ok(deleteUserWithCodeCommandResponse);

        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(StatusCheckQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> StatusCheck(CancellationToken cancellationToken)
        {
            StatusCheckQueryRequest statusCheckQueryRequest = new StatusCheckQueryRequest() { Username = CurrentUsername};
            StatusCheckQueryResponse statusCheckQueryResponse = await _mediator.Send(statusCheckQueryRequest, cancellationToken);

            return Ok(statusCheckQueryResponse);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserPreferencesCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] PreferenceMessage message, CancellationToken cancellationToken)
        {
            UpdateUserPreferencesCommandRequest request = new()
            {
                Message = message.Message,
                Username = CurrentUsername
            };

            UpdateUserPreferencesCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserPreferencesWithTopicsCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserPreferencesWithTopics([FromBody] List<string> topics, CancellationToken cancellationToken)
        {
            var request = new UpdateUserPreferencesWithTopicsCommandRequest
            {
                Username = CurrentUsername,
                Topics = topics
            };
            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }

        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetUserPreferencesCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetUserPreferences(CancellationToken cancellationToken)
        {
            ResetUserPreferencesCommandRequest request = new()
            {
                Username = CurrentUsername
            };

            ResetUserPreferencesCommandResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCountryCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCountry([FromBody] UserCountry country, CancellationToken cancellationToken)
        {
            var request = new UpdateUserCountryCommandRequest
            {
                Country = country.Country,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPreferenceDescriptionsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPreferenceDescriptions(CancellationToken cancellationToken)
        {
            var request = new GetUserPreferenceDescriptionsQueryRequest
            {
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(GetAllTopicsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTopics(CancellationToken cancellationToken)
        {
            var request = new GetAllTopicsQueryRequest();
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCurrencyCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserCurrency([FromBody] UserCurrency currencyCode, CancellationToken cancellationToken)
        {

            var request = new UpdateUserCurrencyCommandRequest
            {
                Username = CurrentUsername,
                CurrencyCode = currencyCode.CurrencyCode
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserCurrencyQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserCurrency(CancellationToken cancellationToken)
        {
            var request = new GetUserCurrencyQueryRequest
            {
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoByNameQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfoByName([FromQuery] string targetUsername, CancellationToken cancellationToken)
        {
            var request = new GetUserInfoByNameQueryRequest()
            {
                Username = CurrentUsername,
                TargetUsername = targetUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoByIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfoById([FromQuery] string userId, CancellationToken cancellationToken)
        {
            var request = new GetUserInfoByIdQueryRequest()
            {
                Username = CurrentUsername,
                UserId = userId
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPhotoQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPhoto(CancellationToken cancellationToken)
        {
            var request = new GetUserPhotoQueryRequest
            {
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPhotoByNameQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPhotoByName([FromQuery] string targetUsername, CancellationToken cancellationToken)
        {
            var request = new GetUserPhotoByNameQueryRequest()
            {
                Username = CurrentUsername,
                TargetUsername = targetUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SetUserPhotoCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SetUserPhoto([FromBody] UserPhoto photo, CancellationToken cancellationToken)
        {
            var request = new SetUserPhotoCommandRequest
            {
                Username = CurrentUsername,
                PhotoUrl = photo.PhotoUrl
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);

        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SearchUsersQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm, CancellationToken cancellationToken)
        {
            var request = new SearchUsersQueryRequest { SearchTerm = searchTerm };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
