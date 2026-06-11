namespace TLearn.Application.Features.Admin.DTOs;

public class AdminPlanRevenueDto

{
    public string PlanType { get; set; } = string.Empty;

    public string PlanName { get; set; } = string.Empty;

    public decimal Revenue { get; set; }

    public int PaidPaymentCount { get; set; }

    public decimal AverageOrderValue { get; set; }

    public decimal RevenuePercentage { get; set; }
}