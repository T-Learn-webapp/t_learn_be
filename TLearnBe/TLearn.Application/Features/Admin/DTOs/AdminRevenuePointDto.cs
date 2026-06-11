namespace TLearn.Application.Features.Admin.DTOs;

public class AdminRevenuePointDto

{
    public string Label { get; set; } = string.Empty;

    public DateTime Date { get; set; }

    public decimal Revenue { get; set; }

    public int PaidPaymentCount { get; set; }
}