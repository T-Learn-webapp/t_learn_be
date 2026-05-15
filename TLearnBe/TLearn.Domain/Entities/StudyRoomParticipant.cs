namespace TLearn.Domain.Entities;

public class StudyRoomParticipant
{
    public Guid Id { get; set; } = Guid.NewGuid();
        
    public Guid StudyRoomId { get; set; }
    public StudyRoom StudyRoom { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public DateTime LastActiveAt { get; set; } = DateTime.UtcNow;
    public bool IsOnline { get; set; } = true;
}