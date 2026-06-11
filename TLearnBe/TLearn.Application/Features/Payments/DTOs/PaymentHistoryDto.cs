namespace TLearn.Application.Features.Payments.DTOs;

public class PaymentHistoryDto

{

    public Guid Id { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public string PlanType { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string? PaymentMethod { get; set; }

    public string? Description { get; set; }

    public string? PayOSPaymentLinkId { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CancelledAt { get; set; }

    public DateTime? ExpiredAt { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public bool IsPaid { get; set; }

    public bool IsPending { get; set; }

    public bool IsCancelled { get; set; }

    public bool IsExpired { get; set; }

}