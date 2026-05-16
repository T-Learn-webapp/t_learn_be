namespace TLearn.Domain.Entities;

public class SubjectInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid SubjectId { get; set; }
    public virtual Subject Subject { get; set; } = null!;
    public string Email { get; set; } = string.Empty;
    public SubjectPermission Permission { get; set; }
    public string InviteToken { get; set; } = string.Empty;
    public Guid InvitedBy { get; set; }
    public virtual User? Inviter { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public DateTime? UsedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int RetryCount { get; set; }
    public DateTime? LastSentAt { get; set; }
    public Guid? AcceptedUserId { get; set; }
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
}
    public enum InvitationStatus
    {
        Pending = 1,      // Chờ xử lý
        Accepted = 2,     // Đã chấp nhận
        Expired = 3,      // Hết hạn
        Cancelled = 4     // Chủ subject hủy
    }