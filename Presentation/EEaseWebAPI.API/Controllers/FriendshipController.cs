using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.Features.Commands.AppUser.BlockFriend;
using EEaseWebAPI.Application.Features.Commands.AppUser.CancelFriendRequest;
using EEaseWebAPI.Application.Features.Commands.AppUser.RemoveFriend;
using EEaseWebAPI.Application.Features.Commands.AppUser.RespondToFriendRequest;
using EEaseWebAPI.Application.Features.Commands.AppUser.SendFriendRequest;
using EEaseWebAPI.Application.Features.Commands.AppUser.UnblockUser;
using EEaseWebAPI.Application.Features.Queries.AppUser.CheckFriendRequest;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetBlockedUsers;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetPendingFriendRequests;
using EEaseWebAPI.Application.Features.Queries.AppUser.GetUserFriends;
using EEaseWebAPI.Application.MapEntities;
using EEaseWebAPI.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]

    [ApiController]
    public class FriendshipController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public FriendshipController(IMediator mediator)
        {
            _mediator = mediator;

        }

        [ProducesResponseType(typeof(SendFriendRequestCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("SendFriendRequest/{targetUsername}")]
        public async Task<IActionResult> SendFriendRequest(string targetUsername)
        {
            if (!TryGetCurrentUsername(out var requesterUsername))
                return Unauthorized();

            var request = new SendFriendRequestCommandRequest
            {
                RequesterUsername = requesterUsername,
                AddresseeUsername = targetUsername
            };

            var response = await _mediator.Send(request);

            return Ok(response);
        }

        [ProducesResponseType(typeof(GetPendingFriendRequestsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("[Action]")]
        public async Task<IActionResult> GetPendingRequests()
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            var query = new GetPendingFriendRequestsQuery { Username = username };
            var response = await _mediator.Send(query);

            return Ok(response);
        }

        [ProducesResponseType(typeof(GetUserFriendsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("[Action]")]
        public async Task<IActionResult> GetFriends()
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            var query = new GetUserFriendsQuery { Username = username };
            var response = await _mediator.Send(query);

            return Ok(response);
        }

        [ProducesResponseType(typeof(RespondToFriendRequestCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPut("RespondToFriendRequest/{requesterUsername}")]
        public async Task<IActionResult> RespondToFriendRequest(string requesterUsername, [FromBody] FriendshipStatus response)
        {
            if (!TryGetCurrentUsername(out var addresseeUsername))
                return Unauthorized();

            var request = new RespondToFriendRequestCommand
            {
                RequesterUsername = requesterUsername,
                AddresseeUsername = addresseeUsername,
                Response = response
            };

            var result = await _mediator.Send(request);

            return Ok(result);
        }

        [ProducesResponseType(typeof(RemoveFriendCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpDelete("RemoveFriend/{friendUsername}")]
        public async Task<IActionResult> RemoveFriend(string friendUsername)
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            var command = new RemoveFriendCommand
            {
                Username = username,
                FriendUsername = friendUsername
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [ProducesResponseType(typeof(BlockFriendCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("BlockUser/{targetUsername}")]
        public async Task<IActionResult> BlockUser(string targetUsername)
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            var command = new BlockFriendCommand
            {
                Username = username,
                TargetUsername = targetUsername
            };

            var response = await _mediator.Send(command);

            return Ok(response);
        }

        [ProducesResponseType(typeof(UnblockUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPost("UnblockUser/{targetUsername}")]
        public async Task<IActionResult> UnblockUser(string targetUsername)
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            UnblockUserCommandRequest request = new UnblockUserCommandRequest()
            {
                username = username,
                targetUsername = targetUsername
            };

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [ProducesResponseType(typeof(GetBlockedUsersQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpGet("[Action]")]
        public async Task<IActionResult> GetBlockedUsers()
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            var query = new GetBlockedUsersQuery { Username = username };
            var response = await _mediator.Send(query);

            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(CancelFriendRequestCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelFriendRequest(string targetUsername)
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            CancelFriendRequestCommandRequest request = new CancelFriendRequestCommandRequest
            {
                username = username,
                targetUsername = targetUsername
            };

            CancelFriendRequestCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(CheckFriendRequestQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CheckFriendRequest(string targetUsername)
        {
            if (!TryGetCurrentUsername(out var username))
                return Unauthorized();

            CheckFriendRequestQueryRequest request = new CheckFriendRequestQueryRequest()
            {
                Username = username,
                TargetUsername = targetUsername
            };

            CheckFriendRequestQueryResponse response = await _mediator.Send(request);
            return Ok(response);
        }

    }

}
