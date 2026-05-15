using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class UserQuizAnswerConfiguration 
    : IEntityTypeConfiguration<UserQuizAnswer>
{
    public void Configure(EntityTypeBuilder<UserQuizAnswer> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.UserQuizResult)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.UserQuizResultId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(x => x.Question)
            .WithMany(x => x.Answers)
            .HasForeignKey(x => x.QuestionId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}