using TLearn.Domain.Entities;

namespace TLearn.Application.Features.FlashCards.DTOs.Requests;

public class UpdateFlashcardProgressRequest
{
    public FlashcardReviewQuality Quality { get; set; }
}