using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.Property(x => x.PlanType)
            .HasMaxLength(100)
            .IsRequired();


        builder.Property(x => x.PlanName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.PayOSOrderCode)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PayOSPaymentLinkId)
            .HasMaxLength(200);

        builder.Property(x => x.Status)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PaymentMethod)
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.RefundedAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(x => x.RefundReason)
            .HasMaxLength(500);

        builder.Property(x => x.LastPayOSStatus)
            .HasMaxLength(100);

        builder.Property(x => x.RawWebhookJson)
            .HasColumnType("nvarchar(max)");

        builder.HasIndex(x => x.PayOSOrderCode)
            .IsUnique();

        builder.HasIndex(x => new { x.UserId, x.Status });

        builder.HasIndex(x => new { x.Status, x.ExpiresAt });

        builder.HasOne(x => x.User)
            .WithMany(x => x.Payments)
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.NoAction);
    }
}