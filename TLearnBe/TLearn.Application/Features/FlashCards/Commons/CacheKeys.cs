namespace TLearn.Application.Features.FlashCards.Commons;

public static class CacheKeys
{
    public static string FlashcardsByMaterial(Guid materialId, Guid userId)
        => $"flashcards:material:{materialId}:user:{userId}";

    public static string FlashcardCountByMaterial(Guid materialId)
        => $"flashcards:material:{materialId}:count";

    public static string FlashcardDetail(Guid flashcardId, Guid userId)
        => $"flashcards:detail:{flashcardId}:user:{userId}";

    public static string DueFlashcards(Guid userId)
        => $"flashcards:due:user:{userId}";
}