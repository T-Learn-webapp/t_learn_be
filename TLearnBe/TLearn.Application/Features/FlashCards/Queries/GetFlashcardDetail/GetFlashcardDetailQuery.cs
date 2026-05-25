using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;

namespace TLearn.Application.Features.FlashCards.Queries.GetFlashcardDetail;

public class GetFlashcardDetailQuery
    : IRequest<Result<FlashcardDetailsDto>>

{
    public Guid FlashcardId { get; set; }

    public GetFlashcardDetailQuery(Guid flashcardId)

    {
        FlashcardId = flashcardId;
    }
}