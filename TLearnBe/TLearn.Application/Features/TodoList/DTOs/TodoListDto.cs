using TLearn.Domain.Entities;

namespace TLearn.Application.Features.TodoList.DTOs;

public class TodoListDto
{
    public Guid Id { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TodoStatus Status { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid LearningMaterialId { get; set; }

    public string LearningMaterialTitle { get; set; } = string.Empty;

    public Guid SubjectId { get; set; }

    public string SubjectName { get; set; } = string.Empty;

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public List<TodoAssignedUserDto> AssignedUsers { get; set; } = [];
}