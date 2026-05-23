using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class TodoAssignmentConfiguration : IEntityTypeConfiguration<TodoAssignment>
{
    public void Configure(EntityTypeBuilder<TodoAssignment> builder)
    {
        builder.HasKey(x => new { x.TodoItemId, x.UserId });

       
        
        builder.HasOne(x => x.TodoItem)
            .WithMany(t => t.Assignments)
            .HasForeignKey(x => x.TodoItemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.User)
            .WithMany(u => u.TodoAssignments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}