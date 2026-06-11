namespace TLearn.Application.Features.Admin.DTOs;

public class AdminRevenueDto

{
    public AdminRevenueSummaryDto Summary { get; set; } = new();

    public List<AdminRevenuePointDto> Chart { get; set; } = new();

    public List<AdminPlanRevenueDto> PlanBreakdown { get; set; } = new();

    public List<AdminRecentPaymentDto> RecentPaidPayments { get; set; } = new();

    public List<AdminTopCustomerDto> TopCustomers { get; set; } = new();

    public DateTime FromDate { get; set; }

    public DateTime ToDate { get; set; }

    public string RangeType { get; set; } = string.Empty;
}