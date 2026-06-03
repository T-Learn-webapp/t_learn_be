namespace TLearn.Domain.Entities;

public class SubscriptionPlan
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string PlanType { get; set; } = string.Empty; 
    // PremiumMonthly, PremiumYearly

    public string PlanName { get; set; } = string.Empty;
    // Vip 1 tháng, Vip 1 năm

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public int DurationDays { get; set; }

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}