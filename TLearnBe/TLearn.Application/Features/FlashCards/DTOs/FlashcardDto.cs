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
    
}