using EEaseWebAPI.Application.Features.Commands.Route.CreateRouteWithoutLogin;
using EEaseWebAPI.Application.Features.Commands.Route.GetRouteComponentPhoto;
using EEaseWebAPI.Application.Features.Commands.Route.LikeRoute;
using EEaseWebAPI.Application.Features.Queries.Route.GetAllRoutes;
using EEaseWebAPI.Application.Features.Queries.Route.GetLikedRoutes;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using EEaseWebAPI.Application.Features.Commands.Route.DeleteRoute;
using EEaseWebAPI.Application.Features.Commands.Route.CreateCustomRoute;
using EEaseWebAPI.Application.DTOs.Route.CreateCustomRoute;
using EEaseWebAPI.Application.Features.Queries.Route.GetRouteById;
using EEaseWebAPI.Application.Features.Commands.Route.UpdateRouteStatus;
using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;
using EEaseWebAPI.Application.DTOs.Route.LikePlaceOrRestaurantDTO;
using EEaseWebAPI.Application.Features.Queries.Route.CheckRouteLikeStatus;
using EEaseWebAPI.Application.DTOs.Route.DislikePlaceOrRestaurantDTO;
using EEaseWebAPI.Application.Features.Commands.Route.DislikePlaceOrRestaurant;
using EEaseWebAPI.Application.Abstractions.Services;
using EEaseWebAPI.Persistence.Services;
using EEaseWebAPI.Domain.Entities.Identity;
using EEaseWebAPI.Domain.Entities.Route;
//using EEaseWebAPI.Application.Features.Commands.Route.LikePlaceOrRestaurant;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RouteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICustomRouteService _customRouteService;

        public RouteController(IMediator mediator,ICustomRouteService customRouteService) 
        {
            _mediator = mediator;
            _customRouteService = customRouteService;

        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CreateRouteWithoutLoginCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateRouteWithoutLogin([FromBody] CreateRouteWithoutLoginCommandRequest request)
        {
            CreateRouteWithoutLoginCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [ProducesResponseType(typeof(GlobalError),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetRouteComponentPhotoCommandResponse),StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRouteComponentPhoto(GetRouteComponentPhotoCommandRequest request) 
        {
            GetRouteComponentPhotoCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(GlobalError),StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetAllRoutesQueryResponse),StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [Authorize(AuthenticationSchemes = "User")]
        public async Task<IActionResult> GetAllRoutes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            GetAllRoutesQueryRequest request = new GetAllRoutesQueryRequest()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Username = requesterUsername

            };
            GetAllRoutesQueryResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetLikedRoutesQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetLikedRoutes([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10) 
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            GetLikedRoutesQueryRequest request = new GetLikedRoutesQueryRequest()
            {
                PageNumber = pageNumber,
                PageSize = pageSize,
                Username = requesterUsername
            };

            GetLikedRoutesQueryResponse response = await _mediator.Send(request);
            return Ok(response);

        }

        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(LikeRouteCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> LikeRoute([FromBody] Guid routeId)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            LikeRouteCommandRequest request = new LikeRouteCommandRequest
            {
                RouteId = routeId,
                Username = requesterUsername
            };

            LikeRouteCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpDelete("[Action]/{routeId}")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(DeleteRouteCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> DeleteRoute([FromRoute] Guid routeId)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            DeleteRouteCommandRequest request = new DeleteRouteCommandRequest
            {
                RouteId = routeId,
                Username = requesterUsername
            };

            DeleteRouteCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }


        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CreateCustomRouteCommandResponse),StatusCodes.Status200OK)]
        public async Task<IActionResult> CreateCustomRoute(CreateCustomRouteDTO createCustomRouteDTO)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

#pragma warning disable CS8601 
            CreateCustomRouteCommandRequest request = new CreateCustomRouteCommandRequest()
            {
                usernames = createCustomRouteDTO.usernames,
                username = requesterUsername,
                PRICE_LEVEL = createCustomRouteDTO.PRICE_LEVEL,
                StartDate = createCustomRouteDTO.StartDate,
                EndDate = createCustomRouteDTO.EndDate,
                destination = createCustomRouteDTO.destination
            };
#pragma warning restore CS8601 

            var response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpGet("[Action]/{routeId}")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetRouteByIdQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetRouteById([FromRoute] Guid routeId)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            GetRouteByIdQueryRequest request = new GetRouteByIdQueryRequest
            {
                RouteId = routeId,
                Username = requesterUsername
            };

            GetRouteByIdQueryResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPut("[Action]/{routeId}")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(UpdateRouteStatusCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> UpdateRouteStatus([FromRoute] Guid routeId, [FromBody] int status)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            UpdateRouteStatusCommandRequest request = new UpdateRouteStatusCommandRequest
            {
                RouteId = routeId,
                Status = status,
                Username = requesterUsername
            };

            UpdateRouteStatusCommandResponse response = await _mediator.Send(request);
            return Ok(response);
        }

        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(LikePlaceOrRestaurantCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> LikePlaceOrRestaurant([FromBody] LikePlaceOrRestaurantEndpointDTO request)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            if (string.IsNullOrEmpty(request.PlaceType))
                return BadRequest("PlaceType is required");

            if (string.IsNullOrEmpty(request.GooglePlaceId))
                return BadRequest("GooglePlaceId is required");

            LikePlaceOrRestaurantCommandRequest commandRequest = new()
            {
                GooglePlaceId = request.GooglePlaceId,
                PlaceType = request.PlaceType,
                Username = requesterUsername
            };

            var response = await _mediator.Send(commandRequest);

            return Ok(response);
        }

        [HttpPost("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(LikePlaceOrRestaurantCommandResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> DislikePlaceOrRestaurant([FromBody] DislikePlaceOrRestaurantDTO request)
        {
            var requesterUsername = User.Identity?.Name;
            if (string.IsNullOrEmpty(requesterUsername))
                return Unauthorized();

            if (string.IsNullOrEmpty(request.PlaceType))
                return BadRequest("PlaceType is required");

            if (string.IsNullOrEmpty(request.GooglePlaceId))
                return BadRequest("GooglePlaceId is required");

            DislikePlaceOrRestaurantCommandRequest commandRequest = new()
            {
                GooglePlaceId = request.GooglePlaceId,
                PlaceType = request.PlaceType,
                Username = requesterUsername,
                DislikeType = request.DislikeType,
                UserFeedback = request.UserFeedback
            };

            var response = await _mediator.Send(commandRequest);

            return Ok(response);
        }

        
        [HttpGet("[Action]/{routeId}")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(CheckRouteLikeStatusQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckRouteLikeStatus([FromRoute] Guid routeId)
        {
            
                var requesterUsername = User.Identity?.Name;
                if (string.IsNullOrEmpty(requesterUsername))
                    return Unauthorized();

                CheckRouteLikeStatusQueryRequest request = new()
                {
                    RouteId = routeId,
                    Username = requesterUsername
                };

                CheckRouteLikeStatusQueryResponse response = await _mediator.Send(request);
  
                return Ok(response);
            
        }

      

    }
}
