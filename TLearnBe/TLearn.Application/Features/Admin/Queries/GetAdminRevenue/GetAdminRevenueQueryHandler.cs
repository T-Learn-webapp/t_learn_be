using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TLearn.Application.Features.Admin.DTOs;
using TLearn.Common;
using TLearn.Domain.Constants;
using TLearn.Infrastructure.Data.Configurations;

namespace TLearn.Application.Features.Admin.Queries.GetAdminRevenue;

public class GetAdminRevenueQueryHandler
    : IRequestHandler<GetAdminRevenueQuery, Result<AdminRevenueDto>>

{
    private readonly TLearnDbContext _context;

    private readonly ILogger<GetAdminRevenueQueryHandler> _logger;

    public GetAdminRevenueQueryHandler(
        TLearnDbContext context,
        ILogger<GetAdminRevenueQueryHandler> logger)

    {
        _context = context;

        _logger = logger;
    }

    public async Task<Result<AdminRevenueDto>> Handle(
        GetAdminRevenueQuery request,
        CancellationToken cancellationToken)

    {
        try

        {
            var now = DateTime.UtcNow;

            var (fromDate, toDate) = ResolveRange(
                request.RangeType,
                request.FromDate,
                request.ToDate,
                now);

            var paymentsCreatedInRange = _context.Payments
                .AsNoTracking()
                .Where(x =>
                    x.CreatedAt >= fromDate &&
                    x.CreatedAt < toDate);

            var paidPaymentsInRange = _context.Payments
                .AsNoTracking()
                .Where(x =>
                    x.Status == PaymentStatuses.Paid &&
                    x.PaidAt.HasValue &&
                    x.PaidAt.Value >= fromDate &&
                    x.PaidAt.Value < toDate);

            var totalRevenue = await paidPaymentsInRange
                .SumAsync(x => (decimal?)x.Amount, cancellationToken) ?? 0m;

            var paidCount = await paidPaymentsInRange
                .CountAsync(cancellationToken);

            var pendingCount = await paymentsCreatedInRange
                .CountAsync(x => x.Status == PaymentStatuses.Pending, cancellationToken);

            var cancelledCount = await paymentsCreatedInRange
                .CountAsync(x => x.Status == PaymentStatuses.Cancelled, cancellationToken);

            var expiredCount = await paymentsCreatedInRange
                .CountAsync(x => x.Status == PaymentStatuses.Expired, cancellationToken);

            var chart = await BuildRevenueChartAsync(
                request.RangeType,
                fromDate,
                toDate,
                cancellationToken);

            var planBreakdown = await BuildPlanBreakdownAsync(
                fromDate,
                toDate,
                totalRevenue,
                cancellationToken);

            var recentPaidPayments = await GetRecentPaidPaymentsAsync(
                fromDate,
                toDate,
                cancellationToken);

            var topCustomers = await GetTopCustomersAsync(
                fromDate,
                toDate,
                cancellationToken);

            var dto = new AdminRevenueDto
            {
                FromDate = fromDate,
                ToDate = toDate,
                RangeType = request.RangeType.ToString(),
                Summary = new AdminRevenueSummaryDto
                {
                    TotalRevenue = totalRevenue,
                    PaidPaymentCount = paidCount,
                    PendingPaymentCount = pendingCount,
                    CancelledPaymentCount = cancelledCount,
                    ExpiredPaymentCount = expiredCount,
                    AverageOrderValue = paidCount == 0
                        ? 0
                        : Math.Round(totalRevenue / paidCount, 2)
                },
                Chart = chart,
                PlanBreakdown = planBreakdown,
                RecentPaidPayments = recentPaidPayments,
                TopCustomers = topCustomers
            };

            return Result<AdminRevenueDto>.Success(dto);
        }

        catch (Exception ex)

        {
            _logger.LogError(ex, "Lỗi khi lấy doanh thu admin");

            return Result<AdminRevenueDto>.Failure(
                "Đã xảy ra lỗi khi lấy doanh thu admin.");
        }
    }

    private async Task<List<AdminPlanRevenueDto>> BuildPlanBreakdownAsync(
        DateTime fromDate,
        DateTime toDate,
        decimal totalRevenue,
        CancellationToken cancellationToken)
    {
        var data = await _context.Payments
            .AsNoTracking()
            .Where(x =>
                x.Status == PaymentStatuses.Paid &&
                x.PaidAt.HasValue &&
                x.PaidAt.Value >= fromDate &&
                x.PaidAt.Value < toDate)
            .GroupBy(x => new
            {
                x.PlanType,
                x.PlanName
            })
            .Select(g => new
            {
                g.Key.PlanType,
                g.Key.PlanName,
                Revenue = g.Sum(x => x.Amount),
                PaidPaymentCount = g.Count()
            })
            .OrderByDescending(x => x.Revenue)
            .ToListAsync(cancellationToken);

        return data
            .Select(x => new AdminPlanRevenueDto
            {
                PlanType = x.PlanType,
                PlanName = x.PlanName,
                Revenue = x.Revenue,
                PaidPaymentCount = x.PaidPaymentCount,
                AverageOrderValue = x.PaidPaymentCount == 0
                    ? 0
                    : Math.Round(x.Revenue / x.PaidPaymentCount, 2),
                RevenuePercentage = totalRevenue == 0
                    ? 0
                    : Math.Round(x.Revenue / totalRevenue * 100, 2)
            })
            .ToList();
    }

    private async Task<List<AdminRecentPaymentDto>> GetRecentPaidPaymentsAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(x =>
                x.Status == PaymentStatuses.Paid &&
                x.PaidAt.HasValue &&
                x.PaidAt.Value >= fromDate &&
                x.PaidAt.Value < toDate)
            .OrderByDescending(x => x.PaidAt)
            .Take(10)
            .Select(x => new AdminRecentPaymentDto
            {
                PaymentId = x.Id,
                OrderCode = x.PayOSOrderCode,
                UserId = x.UserId,
                UserName = x.User.FullName,
                UserEmail = x.User.Email ?? string.Empty,
                PlanType = x.PlanType,
                PlanName = x.PlanName,
                Amount = x.Amount,
                Currency = x.Currency,
                Status = x.Status,
                PaidAt = x.PaidAt,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<List<AdminTopCustomerDto>> GetTopCustomersAsync(
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)
    {
        var data = await _context.Payments
            .AsNoTracking()
            .Where(x =>
                x.Status == PaymentStatuses.Paid &&
                x.PaidAt.HasValue &&
                x.PaidAt.Value >= fromDate &&
                x.PaidAt.Value < toDate)
            .GroupBy(x => new
            {
                x.UserId,
                x.User.FullName,
                x.User.Email
            })
            .Select(g => new AdminTopCustomerDto
            {
                UserId = g.Key.UserId,
                UserName = g.Key.FullName,
                UserEmail = g.Key.Email ?? string.Empty,
                TotalSpent = g.Sum(x => x.Amount),
                PaidPaymentCount = g.Count(),
                LastPaidAt = g.Max(x => x.PaidAt)
            })
            .OrderByDescending(x => x.TotalSpent)
            .Take(10)
            .ToListAsync(cancellationToken);

        return data;
    }

    private static (DateTime FromDate, DateTime ToDate) ResolveRange(
        AdminRevenueRangeType rangeType,
        DateTime? fromDate,
        DateTime? toDate,
        DateTime now)

    {
        return rangeType switch

        {
            AdminRevenueRangeType.Daily => (
                now.Date,
                now.Date.AddDays(1)
            ),

            AdminRevenueRangeType.Monthly => (
                new DateTime(
                    now.Year,
                    now.Month,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                new DateTime(
                    now.Year,
                    now.Month,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc).AddMonths(1)
            ),

            AdminRevenueRangeType.Yearly => (
                new DateTime(
                    now.Year,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc),
                new DateTime(
                    now.Year + 1,
                    1,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc)
            ),

            AdminRevenueRangeType.Custom => (
                fromDate?.Date ?? now.Date,
                (toDate?.Date ?? now.Date).AddDays(1)
            ),

            _ => (
                now.Date,
                now.Date.AddDays(1)
            )
        };
    }

    private async Task<List<AdminRevenuePointDto>> BuildRevenueChartAsync(
        AdminRevenueRangeType rangeType,
        DateTime fromDate,
        DateTime toDate,
        CancellationToken cancellationToken)

    {
        var paidPayments = await _context.Payments
            .AsNoTracking()
            .Where(x =>
                x.Status == PaymentStatuses.Paid &&
                x.PaidAt.HasValue &&
                x.PaidAt.Value >= fromDate &&
                x.PaidAt.Value < toDate)
            .Select(x => new

            {
                PaidAt = x.PaidAt!.Value,

                x.Amount
            })
            .ToListAsync(cancellationToken);

        if (rangeType == AdminRevenueRangeType.Yearly)

        {
            return paidPayments
                .GroupBy(x => new DateTime(
                    x.PaidAt.Year,
                    x.PaidAt.Month,
                    1,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc))
                .OrderBy(g => g.Key)
                .Select(g => new AdminRevenuePointDto

                {
                    Date = g.Key,

                    Label = $"{g.Key.Month:00}/{g.Key.Year}",

                    Revenue = g.Sum(x => x.Amount),

                    PaidPaymentCount = g.Count()
                })
                .ToList();
        }

        return paidPayments
            .GroupBy(x => x.PaidAt.Date)
            .OrderBy(g => g.Key)
            .Select(g => new AdminRevenuePointDto

            {
                Date = g.Key,

                Label = g.Key.ToString("yyyy-MM-dd"),

                Revenue = g.Sum(x => x.Amount),

                PaidPaymentCount = g.Count()
            })
            .ToList();
    }
}