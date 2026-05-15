namespace TLearn.Domain.Entities;

public class StudyRoom
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string RoomCode { get; set; } = string.Empty; // Mã phòng ngắn dễ chia sẻ
    public string? Name { get; set; }
    public string? Description { get; set; }

    public Guid HostUserId { get; set; }
    public User HostUser { get; set; } = null!;

    public Guid? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid? QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime? StartedAt { get; set; }
    public DateTime? EndedAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public ICollection<UserQuizResult> UserQuizResults{ get; set; } 

    public ICollection<StudyRoomParticipant> Participants { get; set; } 
}