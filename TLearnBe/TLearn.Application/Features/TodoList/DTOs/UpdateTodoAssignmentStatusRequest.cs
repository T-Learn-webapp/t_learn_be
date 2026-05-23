namespace TLearn.Application.Features.TodoList.DTOs;

public class UpdateTodoAssignmentStatusRequest

{
    public Guid AssignedUserId { get; set; }

    public string Status { get; set; } = string.Empty;
}