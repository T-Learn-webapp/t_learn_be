using TLearn.Domain.Entities;

namespace TLearn.Application.Features.TodoList.DTOs;

public class TodoCreatedRealtimeDto

{
    public Guid Id { get; set; }

    public Guid LearningMaterialId { get; set; }

    public Guid SubjectId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public DateTime? DueDate { get; set; }

    public Guid CreatedByUserId { get; set; }

    public TodoStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }
}