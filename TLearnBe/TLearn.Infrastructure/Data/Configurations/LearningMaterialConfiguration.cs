using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class LearningMaterialConfiguration
{
    public void Configure(EntityTypeBuilder<LearningMaterial> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);
        
        builder.Property(m => m.Title).IsRequired().HasMaxLength(300);
        builder.HasOne(m => m.Subject).WithMany(s => s.Materials).HasForeignKey(m => m.SubjectId).OnDelete(DeleteBehavior.Cascade);
    }
  
}