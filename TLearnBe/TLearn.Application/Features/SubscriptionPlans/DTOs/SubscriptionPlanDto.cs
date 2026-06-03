namespace TLearn.Application.Features.SubscriptionPlans.DTOs;

public class SubscriptionPlanDto

{
    public Guid Id { get; set; }

    public string PlanType { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public int DurationDays { get; set; }

    public bool IsActive { get; set; }

    public int SortOrder { get; set; }
}