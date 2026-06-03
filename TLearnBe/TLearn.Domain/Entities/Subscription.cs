namespace TLearn.Domain.Entities;

public class Subscription
{
    public Guid Id { get; set; } = Guid.NewGuid();
        
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string PlanType { get; set; } = string.Empty; // PremiumMonthly, PremiumYearly
    
    public string PlanName { get; set; } = string.Empty;
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; } = true;

    public string? PayOSOrderCode { get; set; }
        
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}