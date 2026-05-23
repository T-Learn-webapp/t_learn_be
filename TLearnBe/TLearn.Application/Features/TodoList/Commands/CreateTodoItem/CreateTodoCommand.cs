using MediatR;
using TLearn.Application.Features.TodoList.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;

namespace TLearn.Application.Features.TodoList.Commands.CreateTodoItem;

public class CreateTodoCommand : IRequest<Result<TodoItemDto>>

{

    
    public Guid LearningMaterialId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public List<Guid> AssignedUserIds { get; set; } = [];

}