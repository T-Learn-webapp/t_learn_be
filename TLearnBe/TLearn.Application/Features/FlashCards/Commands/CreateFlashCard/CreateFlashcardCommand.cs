using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Commands.CreateFlashCard;

public class CreateFlashcardCommand : IRequest<Result<FlashcardDto>>
{
    public Guid MaterialId { get; set; }

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }

    public bool IsAIGenerated { get; set; } = false;
}