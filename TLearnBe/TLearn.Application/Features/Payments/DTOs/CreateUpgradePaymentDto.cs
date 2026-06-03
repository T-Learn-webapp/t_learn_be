namespace TLearn.Application.Features.Payments.DTOs;

public class CreateUpgradePaymentDto

{
    public Guid PaymentId { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public string? PaymentLinkId { get; set; }

    public string? CheckoutUrl { get; set; }

    public string? QrCode { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public string PlanType { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public DateTime? ExpiresAt { get; set; }
}