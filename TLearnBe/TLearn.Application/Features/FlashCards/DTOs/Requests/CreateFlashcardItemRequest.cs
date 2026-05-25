namespace TLearn.Application.Features.FlashCards.DTOs.Requests;

public class CreateFlashcardItemRequest
{
    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }

    public bool IsAIGenerated { get; set; } = false;
}