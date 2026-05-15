namespace TLearn.Domain.Entities;

public class UserQuizAnswer
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserQuizResultId { get; set; }
    public UserQuizResult UserQuizResult { get; set; } = null!;

    public Guid QuestionId { get; set; }
    public Question Question { get; set; } = null!;

    public string? UserAnswer { get; set; }
    public int? SelectedOptionIndex { get; set; }
    public bool IsCorrect { get; set; }
    public int? TimeSpentSeconds { get; set; }
}