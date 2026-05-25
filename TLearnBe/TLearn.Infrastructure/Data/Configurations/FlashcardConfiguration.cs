using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class FlashcardConfiguration : IEntityTypeConfiguration<Flashcard>
{
    public void Configure(EntityTypeBuilder<Flashcard> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Front).IsRequired().HasMaxLength(500);
        builder.Property(x => x.Back).IsRequired().HasMaxLength(1000);

        builder.HasOne(x => x.Material)
            .WithMany(m => m.Flashcards)
            .HasForeignKey(x => x.MaterialId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(x => x.CreatedByUser)
            .WithMany()
            .HasForeignKey(x => x.CreatedByUserId).OnDelete(DeleteBehavior.NoAction);
       
    }
}