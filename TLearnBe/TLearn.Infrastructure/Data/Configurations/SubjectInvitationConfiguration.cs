using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class SubjectInvitationConfiguration : IEntityTypeConfiguration<SubjectInvitation>
{
    public void Configure(EntityTypeBuilder<SubjectInvitation> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.Email);

        builder.Property(x => x.InviteToken)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.InviteToken)
            .IsUnique();

        builder.Property(x => x.Permission)
            .HasConversion<int>();

        builder.Property(x => x.ExpiresAt)
            .IsRequired();

        builder.HasIndex(x => x.ExpiresAt);

        builder.Property(x => x.CreatedAt)
            .HasDefaultValueSql("GETUTCDATE()");

        // Relationships
        builder.HasOne(x => x.Subject)
            .WithMany(s => s.Invitations)
            .HasForeignKey(x => x.SubjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Inviter)
            .WithMany()
            .HasForeignKey(x => x.InvitedBy)
            .OnDelete(DeleteBehavior.Restrict);
    }
}