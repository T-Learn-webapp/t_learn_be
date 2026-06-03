using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanType).IsRequired().HasMaxLength(50);
        builder.Property(x => x.PlanName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PayOSOrderCode)
            .HasMaxLength(100);

        builder.HasIndex(x => x.PayOSOrderCode)
            .IsUnique()
            .HasFilter("[PayOSOrderCode] IS NOT NULL");

        builder.HasIndex(x => new { x.UserId, x.IsActive, x.EndDate });

        builder.HasOne(x => x.User)
            .WithMany(u => u.Subscriptions)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}