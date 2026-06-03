namespace TLearn.Domain.Entities;

public class Flashcard
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }

    public bool IsAIGenerated { get; set; } = false;

    public Guid MaterialId { get; set; }

    public LearningMaterial Material { get; set; } = null!;

    public Guid CreatedByUserId { get; set; }

    public User CreatedByUser { get; set; } = null!;

    public ICollection<UserFlashcardProgress> UserProgresses { get; set; }
        = new List<UserFlashcardProgress>();

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }
}

public class UserFlashcardProgress
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid FlashcardId { get; set; }

    public Flashcard Flashcard { get; set; } = null!;

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public double EaseFactor { get; set; } = 2.5;

    public int Interval { get; set; } = 1;

    public int RepetitionCount { get; set; } = 0;

    public DateTime? NextReviewDate { get; set; }

    public DateTime? LastReviewedAt { get; set; }

    public FlashcardReviewQuality? LastQuality { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
    
    public bool IsDeleted { get; set; } = false;

    public DateTime? DeletedAt { get; set; }

    public Guid? DeletedByUserId { get; set; }
}

public enum FlashcardReviewQuality
{
    Again = 1, // Không nhớ
    Good = 2   // Nhớ
}