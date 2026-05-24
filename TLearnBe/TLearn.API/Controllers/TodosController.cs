using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TLearn.Application.Features.TodoList.Commands.AssignTodoMember;
using TLearn.Application.Features.TodoList.Commands.CreateTodoItem;
using TLearn.Application.Features.TodoList.Commands.DeleteTodoItem;
using TLearn.Application.Features.TodoList.Commands.UpdateTodoItem;
using TLearn.Application.Features.TodoList.Commands.UpdateTodoStatus;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Application.Features.TodoList.Queries;
using TLearn.Application.Features.TodoList.Queries.GetTodoDetails;
using TLearn.Common;
using TLearn.Common.Pagination;

namespace TLearn.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodosController : ControllerBase

{
    private readonly IMediator _mediator;

    public TodosController(IMediator mediator)

    {
        _mediator = mediator;
    }

    // =========================

    // CREATE TODO

    // =========================

    [HttpGet("{id:guid}")]

    public async Task<IActionResult> GetDetail(
        Guid id,
        CancellationToken ct)
    {

        var result = await _mediator.Send(
            new GetTodoDetailQuery(id),
            ct);
        if (!result.IsSuccess)
        {
            return NotFound(result);
        }

        return Ok(result);

    }
    
    [HttpPost]
    public async Task<ActionResult<Result<TodoItemDto>>> Create(
        [FromBody] CreateTodoCommand command)

    {
        var result = await _mediator.Send(command);

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }


    [HttpPost("{todoId:guid}/assign-members")]
    public async Task<IActionResult> AssignMembers(
        Guid todoId,
        [FromBody] AssignTodoMemberRequest request)
    {
        var result = await _mediator.Send(
            new AssignTodoMemberCommand
            {
                TodoId = todoId,

                UserIds = request.UserIds
            });

        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }
    // =========================

    // UPDATE TODO

    // =========================

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Result<TodoItemDto>>> Update(
        Guid id,
        [FromBody] UpdateTodoCommand command)
    {
        if (id != command.TodoId)
        {
            return BadRequest(Result<TodoItemDto>
                .Failure("TodoId không hợp lệ"));
        }

        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }


    [HttpPut("{todoId:guid}/assignment-status")]
    public async Task<IActionResult> UpdateAssignmentStatus(
        Guid todoId,
        [FromBody] UpdateTodoAssignmentStatusRequest request)

    {
        var result = await _mediator.Send(
            new UpdateTodoAssignmentStatusCommand

            {
                TodoId = todoId,

                AssignedUserId = request.AssignedUserId,

                Status = request.Status
            });

        if (!result.IsSuccess)

        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // =========================

    // DELETE TODO

    // =========================

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult<Result<bool>>> Delete(Guid id)
    {
        var command = new DeleteTodoCommand
        {
            TodoId = id
        };
        var result = await _mediator.Send(command);
        if (!result.IsSuccess)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    // =========================

    // GET TODOS

    // =========================

    [HttpGet]
    public async Task<ActionResult<Result<PagedResult<TodoItemDto>>>> GetList(
        [FromQuery] TodoQueryParams queryParams)

    {
        var query = new GetTodoListQuery

        {
            Params = queryParams
        };

        var result = await _mediator.Send(query);

        return Ok(result);
    }
}