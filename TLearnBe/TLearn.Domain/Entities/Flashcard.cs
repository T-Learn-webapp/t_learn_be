namespace TLearn.Domain.Entities;

public class Flashcard
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Front { get; set; } = string.Empty;
    public string Back { get; set; } = string.Empty;
    public string? Hint { get; set; }
    public bool IsAIGenerated { get; set; } = false;

    public double EaseFactor { get; set; } = 2.5;
    public int Interval { get; set; } = 1;
    public int RepetitionCount { get; set; } = 0;
    public DateTime? NextReviewDate { get; set; }

    public Guid MaterialId { get; set; }
    public LearningMaterial Material { get; set; } = null!;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}