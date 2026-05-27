namespace TLearn.Application.Features.FlashCards.DTOs;

public class AiGeneratedFlashcardDto

{
    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }
}