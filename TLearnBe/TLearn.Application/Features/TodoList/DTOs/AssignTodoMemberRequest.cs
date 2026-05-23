namespace TLearn.Application.Features.TodoList.DTOs;

public class AssignTodoMemberRequest
{
    public List<Guid> UserIds { get; set; } = [];
}