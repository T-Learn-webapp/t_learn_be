using TLearn.Domain.Entities;

namespace TLearn.Application.Features.TodoList.DTOs;

public class TodoAssignedUserDto
{
    public Guid UserId { get; set; }

    public TodoStatus Status { get; set; }

}