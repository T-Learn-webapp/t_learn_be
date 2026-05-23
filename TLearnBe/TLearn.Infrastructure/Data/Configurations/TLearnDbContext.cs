using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class TLearnDbContext : IdentityDbContext<User, Role, Guid>
{
    public TLearnDbContext(DbContextOptions<TLearnDbContext> options) : base(options) { }

    // DbSets
    public DbSet<Subject> Subjects { get; set; }
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<LearningMaterial> LearningMaterials { get; set; }
    public DbSet<Flashcard> Flashcards { get; set; }
    public DbSet<Question> Questions { get; set; }
    public DbSet<Quiz> Quizzes { get; set; }
    public DbSet<UserProgress> UserProgresses { get; set; }
    public DbSet<StudyRoom> StudyRooms { get; set; }
    public DbSet<StudyRoomParticipant> StudyRoomParticipants { get; set; }
    public DbSet<Subscription> Subscriptions { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<UserQuizResult> UserQuizResults { get; set; }
    public DbSet<SubjectMember> SubjectMembers { get; set; }
    public DbSet<SubjectInvitation> SubjectInvitations { get; set; }
    public DbSet<UserQuizAnswer> UserQuizAnswers { get; set; }
    public DbSet<TodoAssignment> TodoAssignments{ get; set; }
    public DbSet<TodoItem> TodoItems  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(TLearnDbContext).Assembly);
    }
}