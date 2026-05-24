using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class TodoItemConfiguration : IEntityTypeConfiguration<TodoItem>
{
    public void Configure(EntityTypeBuilder<TodoItem> builder)
    {
        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasOne(x => x.LearningMaterial)
            .WithMany()
            .HasForeignKey(x => x.LearningMaterialId)
            .OnDelete(DeleteBehavior.NoAction);
        
        builder.HasOne(x => x.CreatedBy)

            .WithMany()

            .HasForeignKey(x => x.CreatedByUserId)

            .OnDelete(DeleteBehavior.NoAction);
    }
}