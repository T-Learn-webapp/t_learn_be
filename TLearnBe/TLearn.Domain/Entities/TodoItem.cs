namespace TLearn.Domain.Entities;

public class TodoItem
{

    public Guid Id { get; set; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    public DateTime? DueDate { get; set; }

    // Material chứa task

    public Guid LearningMaterialId { get; set; }

    public virtual LearningMaterial LearningMaterial { get; set; } = null!;

    // Người tạo task

    public Guid CreatedByUserId { get; set; }

    public virtual User CreatedBy { get; set; } = null!;

    // Những người được giao

    public virtual ICollection<TodoAssignment> Assignments { get; set; }

        = new List<TodoAssignment>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    
    // Soft delete

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }

}

public class TodoAssignment
{

    public Guid TodoItemId { get; set; }
    public virtual TodoItem TodoItem { get; set; } = null!;
    // Chỉ được assign cho member trong Subject
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    // Trạng thái riêng từng người
    public TodoStatus Status { get; set; } = TodoStatus.Pending;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

    public DateTime? CompletedAt { get; set; }
    
    // Soft remove

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }

    
}

public enum TodoStatus
{
    Pending = 1,    // Chưa làm
    InProgress = 2, // Đang làm
    Completed = 3   // Đã xong
}