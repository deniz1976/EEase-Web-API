using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.DTOs.Route;
using EEaseWebAPI.Application.DTOs.Route.CreateCustomRoute;
using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Application.Features.Commands.Route.DeleteAllRoutes;
using EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute;
using EEaseWebAPI.Application.Features.Commands.Route.LikeRoute;
using EEaseWebAPI.Application.Features.Commands.Route.UnlikeRoute;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;
using EEaseWebAPI.Application.Features.Queries.Route.CheckRouteLikeStatus;
using EEaseWebAPI.Application.Features.Queries.Route.GetAllRoutes;
using EEaseWebAPI.Application.Features.Queries.Route.GetLikedRoutes;
using EEaseWebAPI.Application.Features.Queries.Route.GetRouteById;
using EEaseWebAPI.Application.Features.Queries.Route.GetRoutesByUserId;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/routes")]
    [ApiController]
    public class RoutesController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public RoutesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(CreateCustomRouteCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> CreateRoute(
            [FromBody] CreateCustomRouteDTO createCustomRouteDTO, CancellationToken cancellationToken)
        {
            var request = new CreateCustomRouteCommandRequest
            {
                Usernames = createCustomRouteDTO.Usernames,
                Username = CurrentUsername,
                PRICE_LEVEL = createCustomRouteDTO.PRICE_LEVEL,
                StartDate = createCustomRouteDTO.StartDate,
                EndDate = createCustomRouteDTO.EndDate,
                Destination = createCustomRouteDTO.Destination
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("guest")]
        [ProducesResponseType(typeof(CreateRouteWithoutLoginCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> CreateGuestRoute(
            [FromBody] CreateRouteWithoutLoginCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetAllRoutesQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRoutes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new GetAllRoutesQueryRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(DeleteAllRoutesCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteAllRoutes(CancellationToken cancellationToken)
        {
            var request = new DeleteAllRoutesCommandRequest { Username = CurrentUsername };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("liked")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetLikedRoutesQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLikedRoutes(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new GetLikedRoutesQueryRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{routeId:guid}")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetRouteByIdQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRouteById(
            [FromRoute] Guid routeId, CancellationToken cancellationToken)
        {
            var request = new GetRouteByIdQueryRequest
            {
                RouteId = routeId,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{routeId:guid}")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(DeleteRouteCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRoute(
            [FromRoute] Guid routeId, CancellationToken cancellationToken)
        {
            var request = new DeleteRouteCommandRequest
            {
                RouteId = routeId,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("{routeId:guid}/likes/me")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(LikeRouteCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> LikeRoute(
            [FromRoute] Guid routeId, CancellationToken cancellationToken)
        {
            var request = new LikeRouteCommandRequest
            {
                RouteId = routeId,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpDelete("{routeId:guid}/likes/me")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UnlikeRouteCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UnlikeRoute(
            [FromRoute] Guid routeId, CancellationToken cancellationToken)
        {
            var request = new UnlikeRouteCommandRequest
            {
                RouteId = routeId,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("{routeId:guid}/likes/me")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(CheckRouteLikeStatusQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMyRouteLike(
            [FromRoute] Guid routeId, CancellationToken cancellationToken)
        {
            var request = new CheckRouteLikeStatusQueryRequest
            {
                RouteId = routeId,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPut("{routeId:guid}/visibility")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(UpdateRouteStatusCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRouteVisibility(
            [FromRoute] Guid routeId,
            [FromBody] RouteVisibilityUpdate visibility,
            CancellationToken cancellationToken)
        {
            var request = new UpdateRouteStatusCommandRequest
            {
                RouteId = routeId,
                Status = visibility.Status,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpGet("/api/users/by-id/{userId}/routes")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetRoutesByUserIdQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRoutesByUserId(
            [FromRoute] string userId,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new GetRoutesByUserIdQueryRequest
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                UserId = userId,
                RequesterUsername = CurrentUsername
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
