using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class StudyRoomParticipantConfiguration : IEntityTypeConfiguration<StudyRoomParticipant>
{
    public void Configure(EntityTypeBuilder<StudyRoomParticipant> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.StudyRoom)
            .WithMany(r => r.Participants)
            .HasForeignKey(x => x.StudyRoomId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}