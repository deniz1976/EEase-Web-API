using EEaseWebAPI.Application.Features.Queries.Currency.GetCurrencies;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using System.Security.Claims;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [EnableRateLimiting("global_key")]
    public class CurrencyController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CurrencyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Retrieves all currencies with pagination support.
        /// </summary>
        /// <param name="pageNumber">Page number (default: 1)</param>
        /// <param name="pageSize">Number of items per page (default: 10, maximum: 50)</param>
        /// <returns>Paginated list of currencies</returns>
        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = "User")]
        [ProducesResponseType(typeof(GlobalError), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetCurrenciesQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetCurrencies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10)
        {
            var userName = User.FindFirst(ClaimTypes.Name)?.Value;
            GetCurrenciesQueryRequest request = new()
            {
                username = userName,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            GetCurrenciesQueryResponse response = await _mediator.Send(request);
            return Ok(response);
        }
    }
}
