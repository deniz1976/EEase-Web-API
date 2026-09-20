using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.Features.Queries.AllWorldCities.GetAllCountries;
using EEaseWebAPI.Application.Features.Queries.Cities.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/cities")]
    [ApiController]
    public class CitiesController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public CitiesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetCitiesBySearchQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCities(
            [FromQuery] string search,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetCitiesBySearchQueryRequest
            {
                SearchTerm = search,
                PageSize = pageSize,
                PageNumber = pageNumber
            }, cancellationToken);

            return Ok(response);
        }

        /// <summary>The same search, ordered by what the caller has said they like.</summary>
        [HttpGet("recommended")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetCitiesBySearchQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetRecommendedCities(
            [FromQuery] string search,
            [FromQuery] int pageSize = 10,
            [FromQuery] int pageNumber = 1,
            CancellationToken cancellationToken = default)
        {
            var response = await _mediator.Send(new GetCitiesBySearchQueryRequest
            {
                SearchTerm = search,
                PageSize = pageSize,
                PageNumber = pageNumber,
                Username = CurrentUsername
            }, cancellationToken);

            return Ok(response);
        }
    }

    [Route("api/countries")]
    [ApiController]
    public class CountriesController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public CountriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetAllCountriesQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCountries(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllCountriesQueryRequest(), cancellationToken);
            return Ok(response);
        }
    }
}
