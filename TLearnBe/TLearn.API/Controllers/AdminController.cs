using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Admin.DTOs;
using TLearn.Application.Features.Admin.Queries.GetAdminRevenue;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Policy = "AdminOnly")]
public class AdminController : ControllerBase

{
    private readonly IMediator _mediator;

    public AdminController(IMediator mediator)

    {
        _mediator = mediator;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue(
        [FromQuery] AdminRevenueRangeType rangeType = AdminRevenueRangeType.Daily,
        [FromQuery] DateTime? fromDate = null,
        [FromQuery] DateTime? toDate = null,
        CancellationToken cancellationToken = default)

    {
        var result = await _mediator.Send(
            new GetAdminRevenueQuery

            {
                RangeType = rangeType,

                FromDate = fromDate,

                ToDate = toDate
            },
            cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}