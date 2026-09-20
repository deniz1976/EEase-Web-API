using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.Features.Queries.AllWorldCities.GetAllCountries;
using EEaseWebAPI.Application.Features.Queries.Cities.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public CityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(GetCitiesBySearchQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCitiesBySearch([FromQuery] string searchTerm, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1, CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetCitiesBySearchQueryRequest
            {
                SearchTerm = searchTerm,
                PageSize = pageSize,
                PageNumber = pageNumber,
            }, cancellationToken);

            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetCitiesBySearchQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCitiesBySearchWithPreferences([FromQuery] string searchTerm, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1, CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetCitiesBySearchQueryRequest
            {
                SearchTerm = searchTerm,
                PageSize = pageSize,
                PageNumber = pageNumber,
                Username = CurrentUsername
            }, cancellationToken);

            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetAllCountriesQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCountries(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllCountriesQueryRequest(), cancellationToken);
            return Ok(response);
        }
    }
}
