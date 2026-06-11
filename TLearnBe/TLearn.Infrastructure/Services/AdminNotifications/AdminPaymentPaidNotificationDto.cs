namespace TLearn.Infrastructure.Services.AdminNotifications;

public class AdminPaymentPaidNotificationDto

{

    public Guid PaymentId { get; set; }

    public string OrderCode { get; set; } = string.Empty;

    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = "VND";

    public string PlanType { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public DateTime? PaidAt { get; set; }

    public string Message { get; set; } = string.Empty;

}