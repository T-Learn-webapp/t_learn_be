using TLearn.Domain.Entities;

namespace TLearn.Application.Features.FlashCards.DTOs;

public class UserFlashcardProgressDto

{
    public Guid FlashcardId { get; set; }

    public Guid UserId { get; set; }

    public double EaseFactor { get; set; }

    public int Interval { get; set; }

    public int RepetitionCount { get; set; }

    public DateTime? NextReviewDate { get; set; }

    public DateTime? LastReviewedAt { get; set; }

    public FlashcardReviewQuality? LastQuality { get; set; }
}