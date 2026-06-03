using TLearn.Domain.Constants;

namespace TLearn.Domain.Entities;

public class Payment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }

    public User User { get; set; } = null!;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";
    
    public int DurationDays { get; set; }
    

    // Gói mà user mua: PremiumMonthly / PremiumYearly
    public string PlanType { get; set; } = string.Empty;
    public string PlanName { get; set; } = string.Empty;

    public string PayOSOrderCode { get; set; } = string.Empty;

    public string? PayOSPaymentLinkId { get; set; }

    public string Status { get; set; } = PaymentStatuses.Pending;

    public string? PaymentMethod { get; set; }

    public string? Description { get; set; }

    public decimal? RefundedAmount { get; set; }

    public string? RefundReason { get; set; }

    

    
    // Thời gian hết hạn link thanh toán
    public DateTime? ExpiresAt { get; set; }

    // Thời điểm thanh toán thành công
    public DateTime? PaidAt { get; set; }

    // Thời điểm user hủy thanh toán
    public DateTime? CancelledAt { get; set; }

    // Thời điểm backend đánh dấu hết hạn
    public DateTime? ExpiredAt { get; set; }

    // Lưu raw webhook để debug/đối soát
    public string? RawWebhookJson { get; set; }

    // Trạng thái mới nhất lấy từ payOS
    public string? LastPayOSStatus { get; set; }

    // Webhook có thể bắn nhiều lần
    public int WebhookReceivedCount { get; set; } = 0;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? UpdatedAt { get; set; }
}