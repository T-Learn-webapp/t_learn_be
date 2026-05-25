using MediatR;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Commands.DeleteFlashcard;

public class DeleteFlashcardCommand : IRequest<Result<bool>>
{
    public Guid FlashcardId { get; set; }

    public DeleteFlashcardCommand(Guid flashcardId)
    {
        FlashcardId = flashcardId;
    }
}