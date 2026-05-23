using MediatR;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.TodoList.Commands.UpdateTodoStatus;

public record UpdateTodoAssignmentStatusCommand
    : IRequest<Result<TodoItemDto>>

{
    public Guid TodoId { get; init; }
    

    public Guid AssignedUserId { get; init; }

    public string Status { get; init; } = string.Empty;
}