using Microsoft.AspNetCore.Identity;

namespace TLearn.Domain.Entities;

public class User : IdentityUser<Guid>
{
    public string FullName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public string SubscriptionType { get; set; } = "Free";

    // Navigation Properties
    public ICollection<Subject> Subjects { get; set; } 
    public ICollection<LearningMaterial> LearningMaterials { get; set; }
    public ICollection<Quiz> Quizzes { get; set; } 
    public ICollection<UserProgress> Progresses { get; set; } 
    public ICollection<Payment> Payments { get; set; } 
    public ICollection<Subscription> Subscriptions { get; set; } 
    public ICollection<StudyRoom> HostedRooms { get; set; } 
    public ICollection<UserQuizResult> UserQuizResults{ get; set; } 
    
    
    public virtual ICollection<SubjectMember> SubjectMemberships { get; set; } = new List<SubjectMember>();
    
}

public class Role : IdentityRole<Guid>
{
    public Role() { }
    public Role(string roleName) : base(roleName) { }
}