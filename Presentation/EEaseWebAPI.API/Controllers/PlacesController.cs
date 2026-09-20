using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.DTOs.Route.DislikePlaceOrRestaurantDTO;
using EEaseWebAPI.Application.DTOs.Route.LikePlaceOrRestaurantDTO;
using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;
using EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EEaseWebAPI.API.Controllers
{
    /// <summary>
    /// Hotels, restaurants and sights. They used to live under routes and be called route
    /// components, which is a name for where they are shown rather than what they are: a
    /// photo and an opinion both outlive the route they were first seen in.
    /// </summary>
    [Route("api/places")]
    [ApiController]
    public class PlacesController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public PlacesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("photos")]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetRouteComponentPhotoCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlacePhoto(
            [FromQuery] GetRouteComponentPhotoCommandRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Records that the caller liked a place, which shifts the preferences behind their
        /// next route. It is not tied to a route, so no route is named.
        /// </summary>
        [HttpPost("likes")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(LikePlaceOrRestaurantCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> LikePlace(
            [FromBody] LikePlaceOrRestaurantEndpointDTO request, CancellationToken cancellationToken)
        {
            var commandRequest = new LikePlaceOrRestaurantCommandRequest
            {
                GooglePlaceId = request.GooglePlaceId,
                PlaceType = request.PlaceType,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(commandRequest, cancellationToken);
            return Ok(response);
        }

        /// <summary>
        /// Records that the caller disliked a place and swaps it out of the route it was
        /// found in, which is why this one does name a route.
        /// </summary>
        [HttpPost("dislikes")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(LikePlaceOrRestaurantCommandResponse), StatusCodes.Status200OK)]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> DislikePlace(
            [FromBody] DislikePlaceOrRestaurantDTO request, CancellationToken cancellationToken)
        {
            var commandRequest = new DislikePlaceOrRestaurantCommandRequest
            {
                GooglePlaceId = request.GooglePlaceId,
                PlaceType = request.PlaceType,
                Username = CurrentUsername,
                DislikeType = request.DislikeType,
                UserFeedback = request.UserFeedback,
                RouteId = request.RouteId
            };

            var response = await _mediator.Send(commandRequest, cancellationToken);
            return Ok(response);
        }
    }
}
