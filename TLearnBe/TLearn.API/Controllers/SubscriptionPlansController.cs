using MediatR;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.SubscriptionPlans.Queries.GetSubscriptionPlans;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/subscription-plans")]
public class SubscriptionPlansController : ControllerBase

{
    private readonly IMediator _mediator;

    public SubscriptionPlansController(IMediator mediator)

    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetPlans(
        [FromQuery] bool onlyActive = true,
        CancellationToken cancellationToken = default)

    {
        var result = await _mediator.Send(
            new GetSubscriptionPlansQuery

            {
                OnlyActive = onlyActive
            },
            cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}