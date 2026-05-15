using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class SubjectMemberConfiguration : IEntityTypeConfiguration<SubjectMember>
{
    public void Configure(EntityTypeBuilder<SubjectMember> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => new { x.SubjectId, x.UserId })
            .IsUnique();

        builder.Property(x => x.Permission)
            .HasDefaultValue(SubjectPermission.ViewOnly)
            .HasConversion<int>();

        builder.Property(x => x.JoinedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(x => x.Subject)
            .WithMany(s => s.Members)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.User)
            .WithMany(u => u.SubjectMemberships)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Inviter)
            .WithMany()
            .HasForeignKey(x => x.InvitedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}