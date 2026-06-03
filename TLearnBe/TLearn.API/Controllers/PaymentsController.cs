using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Payments.Commands;
using TLearn.Application.Features.Payments.Commands.SyncPaymentStatus;
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