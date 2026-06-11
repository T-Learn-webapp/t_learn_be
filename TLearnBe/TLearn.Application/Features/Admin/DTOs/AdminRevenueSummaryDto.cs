namespace TLearn.Application.Features.Admin.DTOs;

public class AdminRevenueSummaryDto

{
    public decimal TotalRevenue { get; set; }

    public int PaidPaymentCount { get; set; }

    public int PendingPaymentCount { get; set; }

    public int CancelledPaymentCount { get; set; }

    public int ExpiredPaymentCount { get; set; }

    public decimal AverageOrderValue { get; set; }
}