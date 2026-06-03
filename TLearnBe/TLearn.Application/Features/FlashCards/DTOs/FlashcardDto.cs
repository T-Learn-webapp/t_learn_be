namespace TLearn.Application.Features.FlashCards.DTOs;

public class FlashcardDto
{
    public Guid Id { get; set; }

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }

    public bool IsAIGenerated { get; set; }

    public Guid MaterialId { get; set; }

    public Guid CreatedByUserId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
    
    public UserFlashcardProgressDto? Progress { get; set; }
    public FlashcardLearningStatus LearningStatus { get; set; } = FlashcardLearningStatus.NotStudied;
    
}

public enum FlashcardLearningStatus
{
    NotStudied = 1, // Chưa học
    Studied = 2,    // Đã học
    NeedReview = 3, // Cần học lại
    Remembered = 4  // Đã thuộc
}