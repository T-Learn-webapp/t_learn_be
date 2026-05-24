namespace TLearn.Domain.Entities;

public class LearningMaterial
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? Summary { get; set; }

    public Guid SubjectId { get; set; }
    public Subject Subject { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string? YjsSnapshot { get; set; }      // JSON snapshot của Yjs document
    public long Version { get; set; }              // Version number for sync
    public DateTime? LastSyncedAt { get; set; }    // Last time snapshot was saved
    public bool IsCollaborative { get; set; } = true;  // Enable real-time editing
    
    public virtual ICollection<TodoItem> TodoItems { get; set; } = new List<TodoItem>();
    public ICollection<Flashcard> Flashcards { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    
    // Soft delete

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }
}