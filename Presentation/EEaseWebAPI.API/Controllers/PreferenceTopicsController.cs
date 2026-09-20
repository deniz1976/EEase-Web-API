using EEaseWebAPI.Application.Features.Queries.AppUser.GetAllTopics;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace EEaseWebAPI.API.Controllers
{
    /// <summary>
    /// The topics a traveller may pick their preferences from. They belong to nobody, so
    /// they do not hang off a user.
    /// </summary>
    [Route("api/preference-topics")]
    [ApiController]
    public class PreferenceTopicsController : ApiControllerBase
    {
        private readonly IMediator _mediator;

        public PreferenceTopicsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [ProducesResponseType(typeof(GetAllTopicsQueryResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllTopics(CancellationToken cancellationToken)
        {
            var response = await _mediator.Send(new GetAllTopicsQueryRequest(), cancellationToken);
            return Ok(response);
        }
    }
}
