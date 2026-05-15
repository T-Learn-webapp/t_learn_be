using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class StudyRoomConfiguration : IEntityTypeConfiguration<StudyRoom>
{
    public void Configure(EntityTypeBuilder<StudyRoom> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RoomCode).IsRequired().HasMaxLength(20);
        builder.HasIndex(x => x.RoomCode).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(200);

        builder.HasOne(x => x.HostUser)
            .WithMany(u => u.HostedRooms)
            .HasForeignKey(x => x.HostUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Subject)
            .WithMany()
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}