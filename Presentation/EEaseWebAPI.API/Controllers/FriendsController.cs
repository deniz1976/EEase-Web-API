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
    /// <summary>
    /// The people the caller has already become friends with. One controller used to hold
    /// friends, requests and blocks together, which meant one path prefix for three
    /// different things.
    /// </summary>
    [Route("api/friends")]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
    [ApiController]
    public class FriendsController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public FriendsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetUserFriendsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFriends(CancellationToken cancellationToken)
        {
            var query = new GetUserFriendsQuery { Username = CurrentUsername };

            var response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{username}")]
        [ProducesResponseType(typeof(RemoveFriendCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RemoveFriend(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var command = new RemoveFriendCommand
            {
                Username = CurrentUsername,
                FriendUsername = username
            };

            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }
    }

    /// <summary>
    /// The request standing between the caller and one other person. The same path answers
    /// what it is, accepts or refuses it, and withdraws it.
    /// </summary>
    [Route("api/friend-requests")]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
    [ApiController]
    public class FriendRequestsController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public FriendRequestsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Everything waiting for the caller to answer.</summary>
        [HttpGet]
        [ProducesResponseType(typeof(GetPendingFriendRequestsQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetPendingRequests(CancellationToken cancellationToken)
        {
            var query = new GetPendingFriendRequestsQuery { Username = CurrentUsername };

            var response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpPost("{username}")]
        [ProducesResponseType(typeof(SendFriendRequestCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> SendFriendRequest(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var request = new SendFriendRequestCommandRequest
            {
                RequesterUsername = CurrentUsername,
                AddresseeUsername = username
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Where the caller stands with this person: none, pending, friends, blocked.</summary>
        [HttpGet("{username}")]
        [ProducesResponseType(typeof(CheckFriendRequestQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetFriendRequestStatus(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var request = new CheckFriendRequestQueryRequest
            {
                Username = CurrentUsername,
                TargetUsername = username
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>Answers a request this person sent to the caller.</summary>
        [HttpPut("{username}")]
        [ProducesResponseType(typeof(RespondToFriendRequestCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> RespondToFriendRequest(
            [FromRoute] string username,
            [FromBody] FriendshipStatus response,
            CancellationToken cancellationToken)
        {
            var request = new RespondToFriendRequestCommand
            {
                RequesterUsername = username,
                AddresseeUsername = CurrentUsername,
                Response = response
            };

            var result = await _mediator.Send(request, cancellationToken);
            return Ok(result);
        }

        /// <summary>Withdraws a request the caller sent to this person.</summary>
        [HttpDelete("{username}")]
        [ProducesResponseType(typeof(CancelFriendRequestCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> CancelFriendRequest(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var request = new CancelFriendRequestCommandRequest
            {
                Username = CurrentUsername,
                TargetUsername = username
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }

    [Route("api/blocked-users")]
    [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
    [ApiController]
    public class BlockedUsersController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public BlockedUsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetBlockedUsersQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetBlockedUsers(CancellationToken cancellationToken)
        {
            var query = new GetBlockedUsersQuery { Username = CurrentUsername };

            var response = await _mediator.Send(query, cancellationToken);
            return Ok(response);
        }

        [HttpPost("{username}")]
        [ProducesResponseType(typeof(BlockFriendCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> BlockUser(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var command = new BlockFriendCommand
            {
                Username = CurrentUsername,
                TargetUsername = username
            };

            var response = await _mediator.Send(command, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{username}")]
        [ProducesResponseType(typeof(UnblockUserCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> UnblockUser(
            [FromRoute] string username, CancellationToken cancellationToken)
        {
            var request = new UnblockUserCommandRequest
            {
                Username = CurrentUsername,
                TargetUsername = username
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
