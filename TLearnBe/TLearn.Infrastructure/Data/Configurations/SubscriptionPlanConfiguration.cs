using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TLearn.Domain.Constants;
using TLearn.Domain.Entities;

namespace TLearn.Infrastructure.Data.Configurations;

public class SubscriptionPlanConfiguration 
    : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PlanType)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.PlanName)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.Amount)
            .HasColumnType("decimal(18,2)")
            .IsRequired();

        builder.Property(x => x.Currency)
            .HasMaxLength(10)
            .IsRequired();

        builder.HasIndex(x => x.PlanType)
            .IsUnique();
        
        builder.HasData(
            new SubscriptionPlan
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PlanType = SubscriptionPlanTypes.PremiumMonthly,
                PlanName = "Vip 1 tháng",
                Description = "Nâng cấp tài khoản Vip trong 1 tháng",
                Amount = 49000,
                Currency = "VND",
                DurationDays = 30,
                IsActive = true,
                SortOrder = 1,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            },
            new SubscriptionPlan
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                PlanType = SubscriptionPlanTypes.PremiumYearly,
                PlanName = "Vip 1 năm",
                Description = "Nâng cấp tài khoản Vip trong 1 năm",
                Amount = 499000,
                Currency = "VND",
                DurationDays = 365,
                IsActive = true,
                SortOrder = 2,
                CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            }
        );
    }
}