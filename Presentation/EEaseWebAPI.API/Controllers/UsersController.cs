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
        [ProducesResponseType(typeof(GlobalError),StatusCodes.Status500InternalServerError)]
        [HttpPost("[Action]")]
        public async Task<IActionResult> CreateUser(CreateUserCommandRequest createUserCommandRequest)
        {
            CreateUserCommandResponse createUserCommandResponse = await _mediator.Send(createUserCommandRequest);
            return Ok(createUserCommandResponse);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SendVerificationCodeCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> SendVerificationCodeAgain(SendVerificationCodeCommandRequest request)
        {
           SendVerificationCodeCommandResponse response = await _mediator.Send(request);
            return Ok(response);

        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUser(UpdateUserDTO updateUserDTO)
        {
            UpdateUserCommandRequest updateUserCommandRequest = new UpdateUserCommandRequest()
            {
                Username = updateUserDTO.Username,
                Name = updateUserDTO.Name,
                Surname = updateUserDTO.Surname,
                BornDate = updateUserDTO.BornDate,
                Gender = updateUserDTO.Gender,
                bio = updateUserDTO.Bio,
                user = CurrentUsername
            };

            UpdateUserCommandResponse updateUserCommandResponse = await _mediator.Send(updateUserCommandRequest);
            return Ok(updateUserCommandResponse);
        }

        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("[Action]")]
        public async Task<IActionResult> GetUserInfo()
        {
            var userName = CurrentUsername;
            GetUserInfoQueryRequest request = new() { username = userName };
            GetUserInfoQueryResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CheckEmailConfirmedQueryResponse),StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmailConfirmed(CheckEmailConfirmedQueryRequest request)
        {
            CheckEmailConfirmedQueryResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ConfirmEmailUserCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> EmailConfirm(ConfirmEmailUserCommandRequest request)
        {
            ConfirmEmailUserCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DeleteUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> DeleteAccount()
        {

            DeleteUserCommandRequest deleteUserCommandRequest = new DeleteUserCommandRequest()
            { username=  CurrentUsername };
            DeleteUserCommandResponse response = await _mediator.Send(deleteUserCommandRequest);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DeleteUserWithCodeCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> DeleteAccountWithCode([FromBody] DeleteAccountWithCode code)
        {
            DeleteUserWithCodeCommandRequest deleteUserWithCodeCommandRequest = new DeleteUserWithCodeCommandRequest() {username = CurrentUsername,code = code.Code };
            DeleteUserWithCodeCommandResponse deleteUserWithCodeCommandResponse = await _mediator.Send(deleteUserWithCodeCommandRequest);
            return Ok(deleteUserWithCodeCommandResponse);

        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(StatusCheckQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> StatusCheck()
        {
            StatusCheckQueryRequest statusCheckQueryRequest = new StatusCheckQueryRequest() { username = CurrentUsername};
            StatusCheckQueryResponse statusCheckQueryResponse = await _mediator.Send(statusCheckQueryRequest);

            return Ok(statusCheckQueryResponse);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserPreferencesCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserPreferences([FromBody] string message)
        {
            UpdateUserPreferencesCommandRequest request = new()
            {
                Message = message,
                Username = CurrentUsername
            };

            UpdateUserPreferencesCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserPreferencesWithTopicsCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserPreferencesWithTopics([FromBody] List<string> topics)
        {
            var request = new UpdateUserPreferencesWithTopicsCommandRequest
            {
                Username = CurrentUsername,
                Topics = topics
            };
            var result = await _mediator.Send(request);
            return Ok(result);
        }

        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(ResetUserPreferencesCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> ResetUserPreferences()
        {
            ResetUserPreferencesCommandRequest request = new()
            {
                Username = CurrentUsername
            };

            ResetUserPreferencesCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCountryCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateCountry([FromBody] string country)
        {
            var request = new UpdateUserCountryCommandRequest
            {
                Country = country,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPreferenceDescriptionsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPreferenceDescriptions()
        {
            var request = new GetUserPreferenceDescriptionsQueryRequest
            {
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(GetAllTopicsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllTopics()
        {
            var request = new GetAllTopicsQueryRequest();
            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateUserCurrencyCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UpdateUserCurrency([FromBody] string currencyCode)
        {

            var request = new UpdateUserCurrencyCommandRequest
            {
                Username = CurrentUsername,
                CurrencyCode = currencyCode
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserCurrencyQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserCurrency()
        {
            var request = new GetUserCurrencyQueryRequest
            {
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoByNameQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfoByName([FromQuery]string TargetUsername)
        {
            var request = new GetUserInfoByNameQueryRequest()
            {
                username = CurrentUsername,
                targetUsername = TargetUsername
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserInfoByIdQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserInfoById([FromQuery]string UserId)
        {
            var request = new GetUserInfoByIdQueryRequest()
            {
                username = CurrentUsername,
                userId = UserId
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPhotoQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPhoto()
        {
            var request = new GetUserPhotoQueryRequest
            {
                username = CurrentUsername
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetUserPhotoByNameQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetUserPhotoByName([FromQuery]string TargetUsername)
        {
            var request = new GetUserPhotoByNameQueryRequest()
            {
                username = CurrentUsername,
                targetUsername = TargetUsername
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPut("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SetUserPhotoCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SetUserPhoto([FromQuery] string photoPath)
        {
            var request = new SetUserPhotoCommandRequest
            {
                Username = CurrentUsername,
                PhotoUrl = photoPath
            };

            var response = await _mediator.Send(request);
            return Ok(response);

        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(SearchUsersQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SearchUsers([FromQuery] string searchTerm)
        {
            var request = new SearchUsersQueryRequest
            {
                Header = new Header(),
                Body = new SearchUsersQueryRequestBody
                {
                    SearchTerm = searchTerm
                }
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }
    }
}
