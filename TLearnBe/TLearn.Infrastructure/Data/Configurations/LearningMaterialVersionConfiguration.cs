using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class LearningMaterialVersionConfiguration : IEntityTypeConfiguration<LearningMaterialVersion>
{
    public void Configure(EntityTypeBuilder<LearningMaterialVersion> builder)
    {
        builder
            .HasOne(x => x.LearningMaterial)
            .WithMany(x => x.Versions)
            .HasForeignKey(x => x.LearningMaterialId)
            .OnDelete(DeleteBehavior.NoAction);

        builder
            .HasOne(x => x.EditedByUser)
            .WithMany()
            .HasForeignKey(x => x.EditedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder
            .HasIndex(x => new { x.LearningMaterialId, x.VersionNumber })
            .IsUnique();
    }
}