using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.Notifications.Commands.ReadAllNotification;
using TLearn.Application.Features.Notifications.Commands.ReadNotification;
using TLearn.Application.Features.Notifications.Queries;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class NotificationsController : ControllerBase

{
    private readonly IMediator _mediator;

    public NotificationsController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<IActionResult> GetMyNotifications(
        [FromQuery] GetMyNotificationsQuery query)

    {
        var result = await _mediator.Send(query);

        return Ok(result);
    }

    [HttpPut("{notificationId:guid}/read")]
    public async Task<IActionResult> Read(
        Guid notificationId)

    {
        var result = await _mediator.Send(
            new ReadNotificationCommand

            {
                NotificationId = notificationId
            });

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    [HttpPut("read-all")]
    public async Task<IActionResult> ReadAll()

    {
        var result = await _mediator.Send(
            new ReadAllNotificationsCommand());

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }
}