using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Commands.UpdateFlashCard;

public class UpdateFlashcardContentCommand 
    : IRequest<Result<FlashcardDto>>
{
    public Guid FlashcardId { get; set; }

    public string Front { get; set; } = string.Empty;

    public string Back { get; set; } = string.Empty;

    public string? Hint { get; set; }
}