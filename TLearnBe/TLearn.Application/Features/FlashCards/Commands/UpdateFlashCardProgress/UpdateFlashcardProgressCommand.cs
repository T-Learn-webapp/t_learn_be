using MediatR;
using TLearn.Application.Features.FlashCards.DTOs;
using TLearn.Common;
using TLearn.Domain.Entities;

namespace TLearn.Application.Features.FlashCards.Commands.UpdateFlashCardProgress;

public class UpdateFlashcardProgressCommand
    : IRequest<Result<UserFlashcardProgressDto>>
{
    public Guid FlashcardId { get; set; }

    public FlashcardReviewQuality Quality { get; set; }
}