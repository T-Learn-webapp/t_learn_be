namespace TLearn.Domain.Entities;

public class UserQuizResult
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid QuizId { get; set; }
    public Quiz Quiz { get; set; } = null!;

    public Guid? StudyRoomId { get; set; }
    public StudyRoom? StudyRoom { get; set; }

    public decimal Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public decimal Percentage { get; set; }
    public int? TimeTakenSeconds { get; set; }
    public bool? IsPassed { get; set; }
    public int AttemptNumber { get; set; } = 1;

    public DateTime CompletedAt { get; set; } = DateTime.UtcNow;

    public ICollection<UserQuizAnswer> Answers { get; set; }
}