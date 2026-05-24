namespace TLearn.Domain.Entities;

public class Subject
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? Color { get; set; }
    public string? Icon { get; set; }
    public bool IsPublic { get; set; } = true;

    public Guid UserId { get; set; }
    public User User { get; set; }

    public ICollection<LearningMaterial> Materials { get; set; }
    public ICollection<Quiz> Quizzes { get; set; } 
    
    public virtual ICollection<SubjectMember> Members { get; set; } = new List<SubjectMember>();
    public virtual ICollection<SubjectInvitation> Invitations { get; set; } = new List<SubjectInvitation>();
    
    // Soft delete

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }
    
    
    // Helper methods
    public bool CanUserView(Guid userId)
    {
        return IsPublic || UserId == userId || Members.Any(m => m.UserId == userId);
    }
    
    public bool CanUserComment(Guid userId)
    {
        if (UserId == userId) return true;
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        return member != null && (member.Permission == SubjectPermission.Comment || 
                                  member.Permission == SubjectPermission.Edit || 
                                  member.Permission == SubjectPermission.Manage);
    }
    
    public bool CanUserEdit(Guid userId)
    {
        if (UserId == userId) return true;
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        return member != null && (member.Permission == SubjectPermission.Edit || 
                                  member.Permission == SubjectPermission.Manage);
    }
    
    public bool CanUserManage(Guid userId)
    {
        if (UserId == userId) return true;
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        return member != null && member.Permission == SubjectPermission.Manage;
    }
    
    public SubjectPermission? GetUserPermission(Guid userId)
    {
        if (UserId == userId) return SubjectPermission.Manage;
        var member = Members.FirstOrDefault(m => m.UserId == userId);
        return member?.Permission;
    }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}