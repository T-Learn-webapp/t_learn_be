using MediatR;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.TodoList.Commands.UpdateTodoItem;

public class UpdateTodoCommand : IRequest<Result<TodoItemDto>>
{
    
    public Guid TodoId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public List<Guid> AssignedUserIds { get; set; } = [];
}