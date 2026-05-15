namespace TLearn.Domain.Entities;

public class Quiz
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Title { get; set; } = string.Empty;
    public int TotalQuestions { get; set; }
    public int? DurationMinutes { get; set; }
    public bool IsFinalQuiz { get; set; } = false;

    public Guid? SubjectId { get; set; }
    public Subject? Subject { get; set; }

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<Question> Questions { get; set; } 
    public ICollection<UserQuizResult> UserQuizResults{ get; set; } 
    
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}