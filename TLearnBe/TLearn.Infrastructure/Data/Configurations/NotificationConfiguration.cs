using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class NotificationConfiguration

    : IEntityTypeConfiguration<Notification>

{

    public void Configure(

        EntityTypeBuilder<Notification> builder)

    {

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)

            .HasMaxLength(200)

            .IsRequired();

        builder.Property(x => x.Message)

            .HasMaxLength(2000)

            .IsRequired();

        builder.Property(x => x.ActionUrl)

            .HasMaxLength(500);

        builder.Property(x => x.Metadata)

            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => x.IsRead);

        builder.HasIndex(x => x.CreatedAt);

        builder.HasOne(x => x.User)

            .WithMany(x => x.Notifications)

            .HasForeignKey(x => x.UserId)

            .OnDelete(DeleteBehavior.Cascade);

    }

}