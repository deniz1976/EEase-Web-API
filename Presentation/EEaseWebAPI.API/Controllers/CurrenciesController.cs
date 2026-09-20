using EEaseWebAPI.API.Constants;
using EEaseWebAPI.Application.Features.Queries.Currencies.GetCurrencies;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EEaseWebAPI.API.Controllers
{
    [Route("api/currencies")]
    [ApiController]
    public class CurrenciesController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public CurrenciesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = AuthenticationSchemes.User)]
        [ProducesResponseType(typeof(GetCurrenciesQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetCurrencies(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            CancellationToken cancellationToken = default)
        {
            var request = new GetCurrenciesQueryRequest
            {
                Username = CurrentUsername,
                PageNumber = pageNumber,
                PageSize = pageSize
            };

            var response = await _mediator.Send(request, cancellationToken);
            return Ok(response);
        }
    }
}
