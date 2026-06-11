using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Payments.Commands;
using TLearn.Application.Features.Payments.Commands.SyncPaymentStatus;
using TLearn.Application.Features.Payments.Queries.GetPaymentHistory;
using TLearn.Application.Features.Payments.Queries.GetPaymentStatus;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase

{
    private readonly IMediator _mediator;

    public PaymentsController(IMediator mediator)

    {
        _mediator = mediator;
    }

    [Authorize]
    [HttpGet("history")]
    public async Task<IActionResult> GetHistory(
        [FromQuery] string? status,
        [FromQuery] string? planType,
        [FromQuery] string? search,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var userIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrWhiteSpace(userIdValue) ||
            !Guid.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(new
            {
                message = "Chưa đăng nhập."
            });
        }

        var result = await _mediator.Send(
            new GetPaymentHistoryQuery
            {
                UserId = userId,
                Status = status,
                PlanType = planType,
                Search = search,
                PageNumber = pageNumber,
                PageSize = pageSize
            },
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("upgrade")]
    public async Task<IActionResult> CreateUpgradePayment(
        [FromBody] CreateUpgradePaymentCommand command,
        CancellationToken cancellationToken)

    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpGet("{orderCode}/status")]
    public async Task<IActionResult> GetStatus(
        string orderCode,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetPaymentStatusQuery
            {
                OrderCode = orderCode
            },
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPost("{orderCode}/sync")]
    public async Task<IActionResult> SyncStatus(
        string orderCode,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new SyncPaymentStatusCommand
            {
                OrderCode = orderCode
            },
            cancellationToken);

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("payos-webhook")]
    public async Task<IActionResult> PayOSWebhook(
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(Request.Body);

        var rawBody = await reader.ReadToEndAsync(cancellationToken);

        var result = await _mediator.Send(
            new HandlePayOSWebhookCommand

            {
                RawBody = rawBody
            },
            cancellationToken);

        if (!result.IsSuccess)

        {
            return BadRequest(new

            {
                success = false,

                message = result.Error
            });
        }

        return Ok(new

        {
            success = true
        });
    }
}