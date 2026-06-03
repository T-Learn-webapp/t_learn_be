namespace TLearn.Application.Features.Payments.DTOs;

public class PaymentStatusDto

{
    public Guid PaymentId { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? LastPayOSStatus { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public string PlanType { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public bool IsPaid { get; set; }

    public bool IsExpired { get; set; }

    public bool IsCancelled { get; set; }
}