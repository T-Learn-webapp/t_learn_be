using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class FlashCardProgressConfiguration : IEntityTypeConfiguration<UserFlashcardProgress>
{
    public void Configure(EntityTypeBuilder<UserFlashcardProgress> builder)
    {
        builder
            .HasOne(x => x.Flashcard)
            .WithMany(x => x.UserProgresses)
            .HasForeignKey(x => x.FlashcardId)
            .OnDelete(DeleteBehavior.Cascade);

        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(x => new { x.FlashcardId, x.UserId })
            .IsUnique();
    }
}