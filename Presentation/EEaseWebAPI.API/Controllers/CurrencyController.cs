using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.Features.Queries.Currency.GetCurrencies;
using EEaseWebAPI.Application.MapEntities;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CurrencyController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public CurrencyController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("[Action]")]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
        [ProducesResponseType(typeof(GetCurrenciesQueryResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status429TooManyRequests)]
        public async Task<IActionResult> GetCurrencies([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var userName = CurrentUsername;
            GetCurrenciesQueryRequest request = new()
            {
                Username = userName,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
            GetCurrenciesQueryResponse response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
