using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.DTOs.Place;
using EEaseWebAPI.Application.Features.Commands.Place.DislikePlace;
using EEaseWebAPI.Application.Features.Queries.Place.GetPlacePhoto;
using EEaseWebAPI.Application.Features.Commands.Place.LikePlace;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EEaseWebAPI.API.Controllers
{
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
        [ProducesResponseType(typeof(GetPlacePhotoQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetPlacePhoto(
            [FromQuery] GetPlacePhotoQueryRequest request, CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }

        [HttpPost("likes")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(LikePlaceCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> LikePlace(
            [FromBody] LikePlaceRequest request, CancellationToken cancellationToken)
        {
            var commandRequest = new LikePlaceCommandRequest
            {
                GooglePlaceId = request.GooglePlaceId,
                PlaceType = request.PlaceType,
                Username = CurrentUsername
            };

            var response = await _mediator.Send(commandRequest, cancellationToken);
            return Ok(response);
        }

        [HttpPost("dislikes")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(DislikePlaceCommandResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [EnableRateLimiting(RateLimitPolicies.Expensive)]
        public async Task<IActionResult> DislikePlace(
            [FromBody] DislikePlaceRequest request, CancellationToken cancellationToken)
        {
            var commandRequest = new DislikePlaceCommandRequest
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
