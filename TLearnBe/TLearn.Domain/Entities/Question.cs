namespace TLearn.Domain.Entities;

public class Question
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Type { get; set; } = string.Empty; // MCQ, QA, FillBlank
    public string Content { get; set; } = string.Empty;
    public string CorrectAnswer { get; set; } = string.Empty;
    public string? Options { get; set; } // JSON string
    public string? Explanation { get; set; }
    public bool IsAIGenerated { get; set; } = false;

    public Guid? QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    public Guid? MaterialId { get; set; }
    public LearningMaterial? Material { get; set; }
    public ICollection<UserQuizAnswer> Answers { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}