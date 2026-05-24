namespace TLearn.Domain.Entities;

public class SubjectMember
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubjectId { get; set; }
    public virtual Subject Subject { get; set; } = null!;
    public Guid UserId { get; set; }
    public virtual User User { get; set; } = null!;
    public SubjectPermission Permission { get; set; }
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public Guid? InvitedBy { get; set; }
    public virtual User? Inviter { get; set; }
    public DateTime? LastViewedAt { get; set; }
    
    // Soft delete

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }

    public virtual User? DeletedByUser { get; set; }
}

public enum SubjectPermission
{
    ViewOnly = 1,
    Comment = 2,
    Edit = 3,
    Manage = 4
}