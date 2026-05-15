using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.EaseFactor).HasDefaultValue(2.5);
        builder.Property(x => x.Interval).HasDefaultValue(1);
        builder.Property(x => x.AccuracyRate).HasDefaultValue(0);

        builder.HasOne(x => x.User)
            .WithMany(u => u.Progresses)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Flashcard)
            .WithMany()
            .HasForeignKey(x => x.FlashcardId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Question)
            .WithMany()
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}