using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class UserQuizResultConfiguration : IEntityTypeConfiguration<UserQuizResult>
{
    public void Configure(EntityTypeBuilder<UserQuizResult> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Score)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.Percentage)
            .HasColumnType("decimal(5,2)");

        builder.Property(x => x.AttemptNumber)
            .HasDefaultValue(1);

        builder.Property(x => x.CompletedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        builder.HasOne(x => x.User)
            .WithMany(u => u.UserQuizResults)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Quiz)
            .WithMany(q => q.UserQuizResults)
            .HasForeignKey(x => x.QuizId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.StudyRoom)
            .WithMany(sr => sr.UserQuizResults)
            .HasForeignKey(x => x.StudyRoomId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(x => x.Answers)
            .WithOne(a => a.UserQuizResult)
            .HasForeignKey(a => a.UserQuizResultId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}