using TLearn.Domain.Entities;

namespace TLearn.Application.Features.TodoList.DTOs;

public class TodoDetailDto
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

    public string CreatedByUserName { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public List<TodoAssignedUserDetailDto> AssignedUsers { get; set; } = new();
}

public class TodoAssignedUserDetailDto
{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public TodoStatus Status { get; set; }

    public DateTime AssignedAt { get; set; }

    public DateTime? CompletedAt { get; set; }
}