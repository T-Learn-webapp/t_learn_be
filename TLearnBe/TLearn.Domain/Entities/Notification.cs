namespace TLearn.Domain.Entities;

public class Notification
{
    public Guid Id { get; set; } = Guid.NewGuid();
    // Người nhận
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    // Nội dung
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    // TodoAssigned, SubjectInvite, QuizResult...
    public NotificationType Type { get; set; }
    // Link frontend redirect
    public string? ActionUrl { get; set; }
    // Metadata optional
    public string? Metadata { get; set; }
    // Đã đọc chưa
    public bool IsRead { get; set; } = false;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? ReadAt { get; set; }
}

public enum NotificationType
{
    TodoAssigned = 1,
    TodoUpdated = 2,
    TodoCompleted = 3,
    SubjectInvitation = 10,
    QuizCompleted = 20,
    System = 100
}