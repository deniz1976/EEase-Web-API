using EEaseWebAPI.Application.Features.Queries.AllWorldCities.GetAllCountries;
using EEaseWebAPI.Application.Features.Queries.Cities.GetCitiesBySearch;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CityController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CityController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("[Action]")]
        [ProducesResponseType(typeof(GetCitiesBySearchQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCitiesBySearch([FromQuery] string searchTerm, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
        {
            var response = await _mediator.Send(new GetCitiesBySearchQueryRequest
            {
                SearchTerm = searchTerm,
                PageSize = pageSize,
                PageNumber = pageNumber,
                Username = null 
            });

            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(typeof(GetCitiesBySearchQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetCitiesBySearchWithPreferences([FromQuery] string searchTerm, [FromQuery] int pageSize = 10, [FromQuery] int pageNumber = 1)
        {
            var response = await _mediator.Send(new GetCitiesBySearchQueryRequest
            {
                SearchTerm = searchTerm,
                PageSize = pageSize,
                PageNumber = pageNumber,
                Username = User.Identity?.Name 
            });

            return Ok(response);
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(typeof(GetAllCountriesQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> GetAllCountries()
        {
            var response = await _mediator.Send(new GetAllCountriesQueryRequest());
            return Ok(response);
        }
    }
}
