namespace TLearn.Application.Features.Admin.DTOs;

public class AdminTopCustomerDto

{
    public Guid UserId { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string UserEmail { get; set; } = string.Empty;

    public decimal TotalSpent { get; set; }

    public int PaidPaymentCount { get; set; }

    public DateTime? LastPaidAt { get; set; }
}