using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class AiUsageLogConfiguration : IEntityTypeConfiguration<AiUsageLog>
{
    public void Configure(EntityTypeBuilder<AiUsageLog> builder)
    {
        builder
            .HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasIndex(x => new { x.UserId, x.UsedAt });

        builder
            .HasIndex(x => new { x.UserId, x.Feature, x.UsedAt });
       
    }
}