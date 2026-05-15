namespace TLearn.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();
        
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
    public string Currency { get; set; } = "VND";
        
    public string PayOSOrderCode { get; set; } = string.Empty;
    public string? PayOSPaymentLinkId { get; set; }
        
    public string Status { get; set; } = "PENDING"; // PENDING, PAID, FAILED, REFUNDED, CANCELLED
    public string? PaymentMethod { get; set; }
    public string? Description { get; set; }

    public decimal? RefundedAmount { get; set; }
    public string? RefundReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}