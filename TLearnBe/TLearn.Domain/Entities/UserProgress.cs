namespace TLearn.Domain.Entities;

public class UserProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();
        
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid? FlashcardId { get; set; }
    public Flashcard? Flashcard { get; set; }

    public Guid? QuestionId { get; set; }
    public Question? Question { get; set; }

    public Guid? QuizId { get; set; }
    public Quiz? Quiz { get; set; }

    // Spaced Repetition Fields
    public DateTime? LastReviewDate { get; set; }
    public DateTime? NextReviewDate { get; set; }
    public int Interval { get; set; } = 1;
    public double EaseFactor { get; set; } = 2.5;
    public int RepetitionCount { get; set; } = 0;
    public int CorrectStreak { get; set; } = 0;
    public int? LastRating { get; set; } // 1=Again, 2=Hard, 3=Good, 4=Easy

    public decimal AccuracyRate { get; set; } = 0;
}